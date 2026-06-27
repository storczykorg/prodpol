namespace Storczyk.Prodpol.Core.Services

open System.Collections.Generic
open Dapper
open FSharp.Control
open Npgsql
open Storczyk.Async
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Utils

[<RegisterAsTransient(typeof<ICustomerRepository>)>]
[<RegisterAsTransient(typeof<ICustomerReadRepository>)>]
type PgCustomerRepository(dataSource: NpgsqlDataSource) =
    interface ICustomerRepository with
        member this.AddAsync(entity) =
            async {
                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)
                use! scope = conn.BeginTransactionAsync(ct)

                let! result =
                    wrapTask (
                        conn.ExecuteAsync(
                            // language=postgresql
                            "INSERT INTO prodpol.customers
                             (customer_id, email, phone_number, password_hash, email_confirmed,
                              role, name_first, name_last, company_name)
                             VALUES
                             (@id, @email, @phoneNumber, @passwordHash, @emailConfirmed,
                              @role, @nameFirst, @nameLast, @companyName);",
                            {| id = entity.Id
                               email = entity.Email
                               phoneNumber = entity.PhoneNumber |> Option.defaultValue null
                               passwordHash = entity.PasswordHash |> Option.defaultValue null
                               emailConfirmed = entity.EmailConfirmed
                               role = entity.Role |> Option.defaultValue (Nullable())
                               nameFirst = entity.NameFirst |> Option.defaultValue null
                               nameLast = entity.NameLast |> Option.defaultValue null
                               companyName = entity.CompanyName |> Option.defaultValue null |}
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
                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)
                use! scope = conn.BeginTransactionAsync(ct)

                match!
                    conn.ExecuteAsync(
                        // language=postgresql
                        "DELETE FROM prodpol.customers WHERE customer_id = @id;",
                        {| id = key |}
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
                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)
                use! scope = conn.BeginTransactionAsync(ct)

                match!
                    wrapTask (
                        conn.ExecuteAsync(
                            // language=postgresql
                            "UPDATE prodpol.customers
                             SET email = @email,
                                 phone_number = @phoneNumber,
                                 email_confirmed = @emailConfirmed,
                                 role = @role,
                                 name_first = @nameFirst,
                                 name_last = @nameLast,
                                 company_name = @companyName
                             WHERE customer_id = @id;",
                            {| id = key
                               email = entity.Email
                               phoneNumber = entity.PhoneNumber |> Option.defaultValue null
                               emailConfirmed = entity.EmailConfirmed
                               role = entity.Role |> Option.defaultValue (Nullable())
                               nameFirst = entity.NameFirst |> Option.defaultValue null
                               nameLast = entity.NameLast |> Option.defaultValue null
                               companyName = entity.CompanyName |> Option.defaultValue null |}
                        )
                    )
                with
                | 0 -> return raise (NotFoundException "Resource not found.")
                | 1 ->
                    do! scope.CommitAsync()
                    return ()
                | _ -> return raise (InvalidOperationException "Expected affected rows to be between 0 and 1.")
            }

    interface ICustomerReadRepository with
        member this.GetAllAsync(token) =
            async {
                use! conn = dataSource.OpenConnectionAsync(token)

                let! reader =
                    wrapTask (
                        conn.QueryAsync<CustomerRead>(
                            "SELECT * FROM prodpol.customers_with_roles ORDER BY customer_id;"
                        )
                    )

                return reader |> AsAsyncEnumerable
            }

        member this.CountAsync(token) =
            async {
                use! conn = dataSource.OpenConnectionAsync(token)
                return! wrapTask (conn.ExecuteScalarAsync<int64>("SELECT COUNT(*) FROM prodpol.customers;"))
            }

        member this.GetByIdAsync(key) =
            async {
                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)

                return!
                    wrapTask (
                        conn.QuerySingleOrDefaultAsync<CustomerRead>(
                            "SELECT * FROM prodpol.customers_with_roles WHERE customer_id = @id;",
                            param = {| id = key |},
                            commandTimeout = 1000
                        )
                    )
            }
