namespace Storczyk.Prodpol.Core.Services

open System
open System.Collections.Generic
open Dapper
open FSharp.Control
open Npgsql
open Storczyk.Async
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Utils

[<RegisterAsTransient(typeof<IProductReadRepository>)>]
[<RegisterAsTransient(typeof<IProductDescriptionRepository>)>]
type PgProductRepository(dataSource: NpgsqlDataSource) =

    interface IProductReadRepository with
        member this.GetAllAsync(token) =
            async {
                use! conn = dataSource.OpenConnectionAsync(token)

                let! reader =
                    wrapTask (
                        conn.QueryAsync<ProductRead>("SELECT * FROM prodpol.products_omnibus ORDER BY product_id;")
                    )

                return reader |> AsyncResult.AsAsyncEnumerable
            }

        member this.CountAsync(token) =
            async {
                use! conn = dataSource.OpenConnectionAsync(token)
                return! wrapTask (conn.ExecuteScalarAsync<int64>("SELECT COUNT(*) FROM prodpol.products;"))
            }

        member this.GetByIdAsync(key) =
            async {
                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)

                return!
                    wrapTask (
                        conn.QuerySingleOrDefaultAsync<ProductRead>(
                            "SELECT * FROM prodpol.products_omnibus WHERE product_id = @id;",
                            param = {| id = key |}
                        )
                    )
            }

    interface IProductDescriptionRepository with
        member this.AddAsync(entity) =
            async {
                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)
                use! scope = conn.BeginTransactionAsync(ct)

                let! result =
                    wrapTask (
                        conn.ExecuteAsync(
                            // language=postgresql
                            "INSERT INTO prodpol.product_descriptions
                             (product_id, language_code, title, body, is_public, created_at, last_modified_at)
                             VALUES
                             (@productId, @languageCode, @title, @body, @isPublic, @createdAt, @lastModifiedAt);",
                            {| productId = entity.ProductId
                               languageCode = entity.LanguageCode |> Option.defaultValue null
                               title = entity.Title
                               body = entity.Body
                               isPublic = entity.IsPublic
                               createdAt = entity.CreatedAt
                               lastModifiedAt = entity.LastModifiedAt |}
                        )
                    )

                match result with
                | 1 ->
                    do! scope.CommitAsync()
                    return ()
                | 0 -> return raise (NotFoundException "Resource not found.")
                | _ -> return raise (InvalidOperationException $"Invalid query result: {result}")
            }

        member this.DeleteAsync(key) =
            async {
                let productId, languageCode = key
                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)
                use! scope = conn.BeginTransactionAsync(ct)

                match!
                    conn.ExecuteAsync(
                        // language=postgresql
                        "DELETE FROM prodpol.product_descriptions WHERE product_id = @id AND language_code = @lang;",
                        {| id = productId
                           lang = languageCode |> Option.defaultValue null |}
                    )
                with
                | 0 -> return raise (NotFoundException "Resource not found.")
                | 1 ->
                    do! scope.CommitAsync()
                    return ()
                | _ -> return raise (InvalidOperationException "Expected affected rows to be between 0 and 1.")
            }

        member this.UpdateAsync(key, entity) =
            async {
                let productId, languageCode = key
                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)
                use! scope = conn.BeginTransactionAsync(ct)

                match!
                    wrapTask (
                        conn.ExecuteAsync(
                            // language=postgresql
                            "UPDATE prodpol.product_descriptions
                             SET title = @title,
                                 body = @body,
                                 is_public = @isPublic,
                                 last_modified_at = @lastModifiedAt
                             WHERE product_id = @id AND language_code = @lang;",
                            {| id = productId
                               lang = languageCode |> Option.defaultValue null
                               title = entity.Title
                               body = entity.Body
                               isPublic = entity.IsPublic
                               lastModifiedAt = entity.LastModifiedAt |}
                        )
                    )
                with
                | 0 -> return raise (NotFoundException "Resource not found.")
                | 1 ->
                    do! scope.CommitAsync()
                    return ()
                | _ -> return raise (InvalidOperationException "Expected affected rows to be between 0 and 1.")
            }

        member this.GetAllAsync(token) =
            async {
                use! conn = dataSource.OpenConnectionAsync(token)

                let! reader =
                    wrapTask (
                        conn.QueryAsync<ProductDescription>(
                            "SELECT * FROM prodpol.product_descriptions ORDER BY product_id;"
                        )
                    )

                return reader |> AsyncResult.AsAsyncEnumerable
            }

        member this.CountAsync(token) =
            async {
                use! conn = dataSource.OpenConnectionAsync(token)
                return! wrapTask (conn.ExecuteScalarAsync<int64>("SELECT COUNT(*) FROM prodpol.product_descriptions;"))
            }

        member this.GetByIdAsync(key) =
            async {
                let productId, languageCode = key
                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)

                return!
                    wrapTask (
                        conn.QuerySingleOrDefaultAsync<ProductDescription>(
                            "SELECT * FROM prodpol.product_descriptions WHERE product_id = @id AND language_code = @lang;",
                            param =
                                {| id = productId
                                   lang = languageCode |> Option.defaultValue null |}
                        )
                    )
            }
