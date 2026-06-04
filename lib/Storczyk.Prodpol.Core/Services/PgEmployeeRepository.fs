namespace Storczyk.Prodpol.Core.Services

open System
open System.Linq
open Dapper
open FSharp.Control
open Npgsql
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Models.UtilHelpers
open Storczyk.Prodpol.Core.Utils
open Storczyk.Prodpol.Core.Utils.AsyncResult
open Storczyk.Prodpol.Core.Utils.Task

type PgEmployeeRepository(dataSource: NpgsqlDataSource) =
    interface IEmployeesRepository with
        member this.AddAsync(entity) =
            async {
                let! ct = Async.CancellationToken
                let! conn = dataSource.OpenConnectionAsync(ct)

                use! scope = conn.BeginTransactionAsync(ct)

                match!
                    wrap (fun _ ->
                        conn.ExecuteAsync(
                            // language=postgresql
                            "INSERT INTO prodpol.employees 
                    (employee_id, name_first, name_last, email, phone_number, password_hash)
                    VALUES 
                        (@newKey, @nameFirst,
                         @nameLast, @email,
                         @phoneNumber, @passwordHash);",
                            {| newKey = entity.Id
                               nameFirst = entity.NameFirst
                               nameLast = entity.NameLast
                               email = entity.Email
                               phoneNumber = entity.PhoneNumber
                               passwordHash = entity.PasswordHash |}
                        ))
                    |> bind (function
                        | 0 -> Error DatabaseError.NotFound
                        | 1 -> Ok()
                        | _ ->
                            Error(
                                DatabaseError.UnknownException(
                                    InvalidOperationException
                                        "Expected affected rows to be a value between 0 and 1. Evaluate query"
                                )
                            ))
                with
                | Ok() ->
                    do! scope.CommitAsync()
                    return Ok()
                | e ->
                    do! scope.RollbackAsync()
                    return e

            }

        member this.DeleteAsync(key) =
            async {
                let! ct = Async.CancellationToken
                let! conn = dataSource.OpenConnectionAsync(ct)

                use! scope = conn.BeginTransactionAsync(ct)

                match!
                    conn.ExecuteAsync(
                        // language=postgresql
                        "DELETE FROM prodpol.employees 
                        WHERE employee_id = @id;",
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
                            "SELECT * FROM prodpol.employees
                            ORDER BY employee_id;"
                        ))

                // EVALUATE: check for memory leaks
                // TODO: implement proper resource disposal
                return reader |> Result.map _.ReadUnbufferedAsync<Employee>()
            }

        member this.CountAsync(token) =
            async {
                let! conn = dataSource.OpenConnectionAsync(token)
                let! cnt = wrap (fun _ -> conn.ExecuteScalarAsync<int64>("SELECT COUNT(*) FROM prodpol.employees;"))
                return cnt
            }

        member this.GetByIdAsync(key) : AsyncResult<Employee, DatabaseError> =
            async {
                let! ct = Async.CancellationToken
                let! conn = dataSource.OpenConnectionAsync(ct)

                let! emp =
                    wrap (fun () ->
                        conn.QuerySingleOrDefaultAsync<Employee>(
                            "SELECT * FROM prodpol.employees WHERE employee_id = @id;",
                            param = {| id = key |},
                            commandTimeout = 1000
                        ))
                return emp
            }

        member this.UpdateAsync (key: int64) (entity: Employee) : AsyncResult<unit, DatabaseError> =
            async {
                let! ct = Async.CancellationToken
                let! conn = dataSource.OpenConnectionAsync(ct)

                use! scope = conn.BeginTransactionAsync(ct)

                match!
                    conn.ExecuteAsync(
                        // language=postgresql
                        "UPDATE prodpol.employees 
                        SET 
                            employee_id = @newKey,
                            name_first = @nameFirst,
                            name_last = @nameLast,
                            email = @email,
                            phone_number = @phoneNumber,
                            password_hash = @passwordHash
                        WHERE employee_id = @oldId;",
                        {| oldId = key
                           newKey = entity.Id
                           nameFirst = entity.NameFirst
                           nameLast = entity.NameLast
                           email = entity.Email
                           phoneNumber = entity.PhoneNumber
                           passwordHash = entity.PasswordHash |}
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

    interface IEmployeesReadRepository with
        member this.GetAllAsync(token) =
            async {
                let! conn = dataSource.OpenConnectionAsync(token)

                let! reader =
                    wrap (fun _ ->
                        conn.QueryMultipleAsync(
                            """SELECT employee_id, 
                                    employees.role_id, 
                                    email, 
                                    normalized_email, 
                                    name_first, 
                                    name_last, 
                                    full_name, 
                                    normalized_name, 
                                    phone_number, 
                                    password_hash, 
                                    created_at, 
                                    enabled, 
                                    display_name, 
                                    role_name 
                                FROM prodpol.employees
                            LEFT JOIN prodpol.employee_roles er on employees.role_id = er.role_id
                            ORDER BY employee_id
                            """
                        ))

                // EVALUATE: check for memory leaks
                // TODO: implement proper resource disposal
                return reader |> Result.map _.ReadUnbufferedAsync<EmployeeRead>()
            }

        member this.CountAsync(token) =
            async {
                let! conn = dataSource.OpenConnectionAsync(token)
                let! cnt = wrap (fun _ -> conn.ExecuteScalarAsync<int64>("SELECT COUNT(*) FROM prodpol.employees;"))
                return cnt
            }

        member this.GetByIdAsync(key) =
            async {
                let! ct = Async.CancellationToken
                let! conn = dataSource.OpenConnectionAsync(ct)

                let! emp =
                    wrap (fun () ->
                        conn.QuerySingleOrDefaultAsync<EmployeeRead>(
                            "SELECT * FROM prodpol.employees
                             LEFT JOIN prodpol.employee_roles er on employees.role_id = er.role_id
                             WHERE employee_id = (@id);",
                            param = {| id = key |},
                            commandTimeout = 1000
                        ))

                return emp
            }
