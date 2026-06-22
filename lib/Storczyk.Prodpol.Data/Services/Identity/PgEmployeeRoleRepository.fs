namespace Storczyk.Prodpol.Core.Services

open System
open Dapper
open FSharp.Control
open Npgsql
open Storczyk.Async
open Storczyk.Async.AsyncResult
open Storczyk.Async.Task
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Utils

[<RegisterAsTransient(typeof<IEmployeeRoleRepository>)>]
type PgEmployeeRoleRepository(dataSource: NpgsqlDataSource) =
    interface IEmployeeRoleRepository with
        member this.AddAsync(entity) =
            async {
                let! ct = Async.CancellationToken
                let! conn = dataSource.OpenConnectionAsync(ct)

                use! scope = conn.BeginTransactionAsync(ct)

                let! i =
                    wrapTask (
                        conn.ExecuteScalarAsync<int>(
                            // language=postgresql
                            "INSERT INTO prodpol.employee_roles 
                            (display_name, role_name)
                            VALUES 
                                ($displayName, $roleName)
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
                let! conn = dataSource.OpenConnectionAsync(ct)

                use! scope = conn.BeginTransactionAsync(ct)

                match!
                    conn.ExecuteAsync(
                        // language=postgresql
                        "DELETE FROM prodpol.employee_roles 
                        WHERE role_name = $id;",
                        {| id = key |}
                    )
                with
                | 0 ->
                    return raise (NotFoundException "Resource not found.")
                | 1 ->
                    do! scope.CommitAsync()
                    return ()
                | _ ->
                    return raise (
                        InvalidOperationException
                            "Expected affected rows to be a value between 0 and 1. Evaluate query"
                    )
            }

        member this.GetAllAsync(token) =
            async {
                use! conn = dataSource.OpenConnectionAsync(token)

                let sql =
                    """SELECT * FROM prodpol.employee_roles
                    ORDER BY role_id;"""

                let! reader = wrapTask (conn.QueryAsync<EmployeeRole>(sql))
                return reader |> AsAsyncEnumerable
            }

        member this.CountAsync(token) =
            async {
                let! conn = dataSource.OpenConnectionAsync(token)
                return! wrapTask (conn.ExecuteScalarAsync<int64>("SELECT COUNT(*) FROM prodpol.employee_roles;"))
            }

        member this.GetByIdAsync(key) =
            async {
                let! ct = Async.CancellationToken
                let! conn = dataSource.OpenConnectionAsync(ct)

                let! emp =
                    wrapOpt (fun () ->
                        conn.QuerySingleOrDefaultAsync<EmployeeRole option>(
                            "SELECT * FROM prodpol.employee_roles WHERE role_name = @id;",
                            param = {| id = key |},
                            commandTimeout = 1000
                        ))

                return emp
            }

        member this.UpdateAsync (key, entity) =
            async {
                let! ct = Async.CancellationToken
                let! conn = dataSource.OpenConnectionAsync(ct)

                use! scope = conn.BeginTransactionAsync(ct)

                match!
                    conn.ExecuteAsync(
                        // language=postgresql
                        "UPDATE prodpol.employee_roles 
                        SET 
                            role_name = $newKey,
                            role_id = $id,
                            display_name = $displayName
                        WHERE role_name = $oldId",
                        {| oldId = key
                           newKey = entity.RoleName
                           id = entity.Id
                           displayName = entity.DisplayName |}
                    )
                with
                | 0 ->
                    return raise (NotFoundException "Resource not found.")
                | 1 ->
                    do! scope.CommitAsync()
                    return ()
                | _ ->
                    return raise (
                        InvalidOperationException
                            "Expected affected rows to be a value between 0 and 1. Evaluate query"
                    )

            }
