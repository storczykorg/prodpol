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

[<RegisterAsTransient(typeof<IProductRepository>)>]
type ProductsRepository(dataSource: NpgsqlDataSource) =

    interface IProductRepository with
        member this.AddAsync(entity) =
            async {
                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)
                use! scope = conn.BeginTransactionAsync(ct)

                let! result =
                    wrapTask (
                        conn.ExecuteAsync(
                            // language=postgresql
                            "INSERT INTO prodpol.products
                             (product_id, product_name, created_at, created_by,
                              last_modified_by, last_modified_at, price, unit_type,
                              available_amount, unit_base)
                             VALUES
                             (@id, @name, @createdAt, @createdBy,
                              @lastModifiedBy, @lastModifiedAt, @price, @unitType,
                              @availableAmount, @unitBase);",
                            {| id = entity.Id
                               name = entity.Name
                               createdAt = entity.CreatedAt
                               createdBy = entity.CreatedBy
                               lastModifiedBy = entity.LastModifiedBy
                               lastModifiedAt = entity.LastModifiedAt
                               price = entity.Price
                               unitType = entity.UnitType
                               availableAmount = entity.AvailableAmount
                               unitBase = entity.UnitBase |}
                        )
                    )

                match result with
                | 1 ->
                    do! scope.CommitAsync()
                    return ()
                | 0 -> return raise (NotFoundException "Resource not found.")
                | _ -> return raise (InvalidOperationException $"Invalid query result: {result}")
            }

        member this.UpdateAsync(key, entity) =
            async {
                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)
                use! scope = conn.BeginTransactionAsync(ct)

                match!
                    wrapTask (
                        conn.ExecuteAsync(
                            // language=postgresql
                            "UPDATE prodpol.products
                             SET product_name = @name,
                                 price = @price,
                                 unit_type = @unitType,
                                 available_amount = @availableAmount,
                                 unit_base = @unitBase,
                                 last_modified_by = @lastModifiedBy,
                                 last_modified_at = @lastModifiedAt
                             WHERE product_id = @id;",
                            {| id = key
                               name = entity.Name
                               price = entity.Price
                               unitType = entity.UnitType
                               availableAmount = entity.AvailableAmount
                               unitBase = entity.UnitBase
                               lastModifiedBy = entity.LastModifiedBy
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

        member this.DeleteAsync(key) =
            async {
                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)
                use! scope = conn.BeginTransactionAsync(ct)

                match!
                    conn.ExecuteAsync(
                        // language=postgresql
                        "DELETE FROM prodpol.products WHERE product_id = @id;",
                        {| id = key |}
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

                let! reader = wrapTask (conn.QueryAsync<Product>("SELECT * FROM prodpol.products ORDER BY product_id;"))

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
                        conn.QuerySingleOrDefaultAsync<Product>(
                            "SELECT * FROM prodpol.products WHERE product_id = @id;",
                            param = {| id = key |}
                        )
                    )
            }
