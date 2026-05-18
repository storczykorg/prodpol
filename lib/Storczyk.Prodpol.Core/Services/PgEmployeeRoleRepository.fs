namespace Storczyk.Prodpol.Core.Services

open System
open Dapper
open FSharp.Control
open Npgsql
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Utils.AsyncResult
open Storczyk.Prodpol.Core.Utils.Task

type PgEmployeeRoleRepository(dataSource: NpgsqlDataSource) =
    interface IEmployeeRoleRepository with
        member this.AddAsync(entity) =
            async {
                let! ct = Async.CancellationToken
                let! conn = dataSource.OpenConnectionAsync(ct)

                use! scope = conn.BeginTransactionAsync(ct)

                match!
                    wrap (fun _ ->
                        conn.ExecuteScalarAsync<int32>(
                            // language=postgresql
                            "INSERT INTO prodpol.employee_roles 
                            (display_name, role_name)
                            VALUES 
                                ($displayName, $roleName)
                            RETURNING role_id;",
                            {| displayName = entity.DisplayName
                               roleName = entity.RoleName |}
                        ))
                    |> bindIgnore (fun x ->
                        entity.Id <- x
                        async.Return(Ok()))
                with
                | Ok _ ->
                    do! scope.CommitAsync()
                    return Ok()
                | Error e ->
                    do! scope.RollbackAsync()
                    return Error e

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
                    do! scope.RollbackAsync()
                    return Error DatabaseError.NotFound
                | 1 ->
                    do! scope.CommitAsync()
                    return Ok()
                | _ ->
                    do! scope.RollbackAsync()

                    return
                        Error(
                            DatabaseError.UnknownException(
                                InvalidOperationException
                                    "Expected affected rows to be a value between 0 and 1. Evaluate query"
                            )
                        )
            }

        member this.GetAllAsync(token) =
            async {
                let! conn = dataSource.OpenConnectionAsync(token)

                let! reader =
                    wrap (fun _ ->
                        conn.QueryMultipleAsync(
                            "SELECT * FROM prodpol.employee_roles
                    ORDER BY role_id;"
                        ))

                // EVALUATE: check for memory leaks
                // TODO: implement proper resource disposal
                return reader |> Result.map _.ReadUnbufferedAsync<EmployeeRole>()
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

        member this.UpdateAsync key entity =
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
                    do! scope.RollbackAsync()
                    return Error DatabaseError.NotFound
                | 1 ->
                    do! scope.CommitAsync()
                    return Ok()
                | _ ->
                    do! scope.RollbackAsync()

                    return
                        Error(
                            DatabaseError.UnknownException(
                                InvalidOperationException
                                    "Expected affected rows to be a value between 0 and 1. Evaluate query"
                            )
                        )

            }
