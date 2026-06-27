namespace Storczyk.Prodpol.Core.Services

open Dapper
open FSharp.Control
open Npgsql
open Storczyk.Async
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Utils

[<RegisterAsTransient(typeof<ICustomerRoleRepository>)>]
type PgCustomerRoleRepository(dataSource: NpgsqlDataSource) =
    interface ICustomerRoleRepository with
        member this.AddAsync(entity) =
            async {
                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)
                use! scope = conn.BeginTransactionAsync(ct)

                let! i =
                    wrapTask (
                        conn.ExecuteScalarAsync<int>(
                            // language=postgresql
                            "INSERT INTO prodpol.customer_roles (display_name, role_name)
                             VALUES ($displayName, $roleName)
                             RETURNING role_id;",
                            {| displayName = entity.DisplayName
                               roleName = entity.RoleName |}
                        )
                    )

                entity.Id <- i
                do! scope.CommitAsync()
            }

        member this.DeleteAsync(key) =
            async {
                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)
                use! scope = conn.BeginTransactionAsync(ct)

                match!
                    conn.ExecuteAsync(
                        // language=postgresql
                        "DELETE FROM prodpol.customer_roles WHERE role_name = $id;",
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

                let! reader =
                    wrapTask (conn.QueryAsync<CustomerRole>("SELECT * FROM prodpol.customer_roles ORDER BY role_id;"))

                return reader |> AsAsyncEnumerable
            }

        member this.CountAsync(token) =
            async {
                use! conn = dataSource.OpenConnectionAsync(token)
                return! wrapTask (conn.ExecuteScalarAsync<int64>("SELECT COUNT(*) FROM prodpol.customer_roles;"))
            }

        member this.GetByIdAsync(key) =
            async {
                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)

                let! role =
                    wrapTask (
                        conn.QuerySingleOrDefaultAsync<CustomerRole>(
                            "SELECT * FROM prodpol.customer_roles WHERE role_name = @id;",
                            param = {| id = key |},
                            commandTimeout = 1000
                        )
                    )

                return role
            }

        member this.UpdateAsync(key, entity) =
            async {
                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)
                use! scope = conn.BeginTransactionAsync(ct)

                match!
                    conn.ExecuteAsync(
                        // language=postgresql
                        "UPDATE prodpol.customer_roles
                         SET role_name = $newKey,
                             role_id = $id,
                             display_name = $displayName
                         WHERE role_name = $oldId;",
                        {| oldId = key
                           newKey = entity.RoleName
                           id = entity.Id
                           displayName = entity.DisplayName |}
                    )
                with
                | 0 -> return raise (NotFoundException "Resource not found.")
                | 1 ->
                    do! scope.CommitAsync()
                    return ()
                | _ -> return raise (InvalidOperationException "Expected affected rows to be between 0 and 1.")
            }
