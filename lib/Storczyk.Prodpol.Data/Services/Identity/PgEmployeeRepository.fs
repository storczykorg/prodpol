namespace Storczyk.Prodpol.Core.Services

open System
open System.Collections.Generic
open System.Threading.Tasks
open Dapper
open FSharp.Control
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Mvc
open Npgsql
open Storczyk.Async
open Storczyk.Async.AsyncResult
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Utils

[<RegisterAsTransient(typeof<IEmployeesRepository>)>]
[<RegisterAsTransient(typeof<IEmployeesReadRepository>)>]
type PgEmployeeRepository(dataSource: NpgsqlDataSource) =
    let [<Literal>] countByEmailSql =
        """
        SELECT count(*) FROM prodpol.employees_with_roles
        where LOWER(normalized_email) = LOWER((@email)::text)
        or LOWER(email) = LOWER(@email);
        """
    interface IEmployeesRepository with
        member this.AddAsync(entity) =
            async {
                let errors = new List<ValidationErrorDetail>()

                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)
                use! scope = conn.BeginTransactionAsync(ct)

                let! simmilarEmails: int =
                    (wrapTask(
                    conn.ExecuteScalarAsync<int>(
                        countByEmailSql,
                        {| email = entity.Email.ToLower() |})))

                if(simmilarEmails > 0) then
                    errors.Add({ Field = "Email"; Issue = "Email is already present"})


                if(errors.Count > 0) then
                    return raise (ValidationErrorException errors)

                let sql =
                    // language=postgresql
                    "INSERT INTO prodpol.employees
                    (employee_id, name_first, name_last, email, phone_number, password_hash)
                    VALUES
                        (@newKey, @nameFirst,
                         @nameLast, @email,
                         @phoneNumber, @passwordHash);"

                let! result: int =
                    wrapTask (
                        conn.ExecuteAsync(
                            sql,
                            {| newKey = entity.Id
                               nameFirst = entity.NameFirst
                               nameLast = entity.NameLast
                               email = entity.Email
                               phoneNumber = entity.PhoneNumber
                               passwordHash = entity.PasswordHash |> Option.defaultValue null |}
                        ))

                match result with
                | 1 ->
                    do! scope.CommitAsync()
                    return ()
                | 0 ->
                    return raise (NotFoundException "Resource not found.")
                | _ ->
                    raise (Exception $"Invalid query result. Result: {result}\nQuery: \n{sql}")
            }

        member this.DeleteAsync(key) =
            async {
                try
                    let! ct = Async.CancellationToken
                    use! conn = dataSource.OpenConnectionAsync(ct)
                    use! scope = conn.BeginTransactionAsync(ct)
                    let! result =
                        wrapTask (
                            conn.ExecuteAsync(
                                // language=postgresql
                                "DELETE FROM prodpol.employees
                                WHERE employee_id = (@id);",
                                {| id = key |}
                            )
                        )
                    match result with
                    | 0 ->
                        return raise (NotFoundException "Resource not found.")
                    | 1 ->
                        return ()
                    | _ ->
                        return raise (
                            InvalidOperationException
                                "Expected affected rows to be a value between 0 and 1. Evaluate query"
                        )
                with :? PostgresException as ex when ex.SqlState = "23502" ->
                    raise (ReferentialIntegrityException(ex.TableName, ex.ColumnName, ex))
            }

        member this.GetAllAsync(token) =
            async {
                let! conn = dataSource.OpenConnectionAsync(token)

                let! reader =
                    wrapTask (conn.QueryAsync<Employee>(
                            "SELECT * FROM prodpol.employees
                            ORDER BY employee_id;"
                        ))
                return reader |> AsAsyncEnumerable
            }

        member this.CountAsync(token) =
            async {
                let! conn = dataSource.OpenConnectionAsync(token)
                let cnt = wrapTask (conn.ExecuteScalarAsync<int64>("SELECT COUNT(*) FROM prodpol.employees;"))
                return! cnt
            }

        member this.GetByIdAsync(key) =
            async {
                let! ct = Async.CancellationToken
                let! conn = dataSource.OpenConnectionAsync(ct)

                let emp =
                    wrapTask (
                        conn.QuerySingleOrDefaultAsync<Employee>(
                            "SELECT * FROM prodpol.employees WHERE employee_id = @id;",
                            param = {| id = key |},
                            commandTimeout = 1000
                        ))

                return! emp
            }

        member this.UpdateAsync ((key: int64), (entity: Employee)) =
            async {
                let errors = new List<ValidationErrorDetail>()
                let! ct = Async.CancellationToken
                let! conn = dataSource.OpenConnectionAsync(ct)

                use! scope: NpgsqlTransaction = conn.BeginTransactionAsync(ct)

                let! simmilarEmails: int =
                    (wrapTask(
                    conn.ExecuteScalarAsync<int>(
                        """
                        SELECT count(*) from prodpol.employees_with_roles
                        where lower(normalized_email) = lower(@email::TEXT)
                        AND employee_id <> (@key)::bigint;
                        """,
                        {| email = entity.Email.ToLower()
                           key = key |})))

                if(simmilarEmails > 0) then
                    errors.Add({ Field = "Email"; Issue = "Email is already present"})


                if(errors.Count > 0) then
                    return raise (ValidationErrorException errors)

                match!
                    wrapTask(conn.ExecuteAsync(
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
                           passwordHash = entity.PasswordHash |> Option.defaultValue null |}
                    ))
                with
                | 0 ->
                    return raise (NotFoundException "Resource not found.")
                | 1 ->
                    do! scope.CommitAsync()
                | _ ->
                    return raise (
                        InvalidOperationException
                            "Expected affected rows to be a value between 0 and 1. Evaluate query"
                    )

            }

    interface IEmployeesReadRepository with
        member this.GetAllAsync(token) =
            async {
                use! conn = dataSource.OpenConnectionAsync(token)

                let! results =
                    wrapTask (
                        conn.QueryAsync<EmployeeRead>(
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

                return results |> AsAsyncEnumerable
            }

        member this.CountAsync(token) =
            async {
                use! conn = dataSource.OpenConnectionAsync(token)
                let cnt = wrapTask (conn.ExecuteScalarAsync<int64>("SELECT COUNT(*) FROM prodpol.employees;"))
                return! cnt
            }

        member this.GetByIdAsync(key) =
            async {
                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)

                return!   wrapTask (
                        conn.QuerySingleOrDefaultAsync<EmployeeRead>(
                            "SELECT * FROM prodpol.employees
                             LEFT JOIN prodpol.employee_roles er on employees.role_id = er.role_id
                             WHERE employee_id = (@id);",
                            param = {| id = key |},
                            commandTimeout = 1000
                        ))
            }
