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
        /// Adds a new employee to the repository asynchronously.
        ///
        /// Inserts the provided employee entity into the employees table with all relevant
        /// fields including employee_id, names, email, phone number and password hash.
        ///
        /// If the operation succeeds, returns () after committing the transaction. If no rows
        /// were affected (e.g., constraint conflict), returns a DatabaseError.NotFound error.
        /// Any unexpected query results will raise an exception with diagnostic information.
        ///
        /// Returns AsyncResult<(), DatabaseError>
        /// Inserts a new employee record into the PostgreSQL database within a transactional scope.
        /// Executes an INSERT statement against the 'prodpol.employees' table, mapping fields from the
        /// provided Employee entity (Id, NameFirst, NameLast, Email, PhoneNumber, PasswordHash) to the
        /// corresponding database columns. Commits the transaction if exactly one row is inserted.
        /// Returns Ok with unit on success. Returns DatabaseError.NotFound if zero rows are affected.
        /// Raises an Exception for invalid insertion counts. Supports asynchronous cancellation via CancellationToken.
        member this.AddAsync(entity) =
            asyncResult {
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
                    do! scope.RollbackAsync()
                    return! Error (DatabaseError.ValidationErrors errors)

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
                    return! Error DatabaseError.NotFound
                | _ ->
                    raise (Exception $"Invalid query result. Result: {result}\nQuery: \n{sql}")
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
                        WHERE employee_id = (@id);",
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
            asyncResult {
                let! conn = dataSource.OpenConnectionAsync(token)

                let! reader =
                    wrapTask (conn.QueryAsync<Employee>(
                            "SELECT * FROM prodpol.employees
                            ORDER BY employee_id;"
                        ))
                return reader |> AsAsyncEnumerable
            }

        member this.CountAsync(token) : AsyncResult<int64> =
            asyncResult {
                let! conn = dataSource.OpenConnectionAsync(token)
                let cnt = wrapTask (conn.ExecuteScalarAsync<int64>("SELECT COUNT(*) FROM prodpol.employees;"))
                return! cnt
            }

        member this.GetByIdAsync(key) : AsyncResult<Employee, DatabaseError> =
            asyncResult {
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

        member this.UpdateAsync (key: int64) (
            entity: Employee) : AsyncResult<unit> =
            asyncResult {
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
                    do! scope.RollbackAsync()
                    return! Error (DatabaseError.ValidationErrors errors)

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
                    do! scope.RollbackAsync()
                    return! Error DatabaseError.NotFound
                | 1 ->
                    do! scope.CommitAsync()
                | _ ->
                    do! scope.RollbackAsync()

                    return!
                        Error(
                            DatabaseError.UnknownException(
                                InvalidOperationException
                                    "Expected affected rows to be a value between 0 and 1. Evaluate query"
                            )
                        )

            }

    interface IEmployeesReadRepository with
        member this.GetAllAsync(token) =
            asyncResult {
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
            asyncResult {
                use! conn = dataSource.OpenConnectionAsync(token)
                let cnt = wrapTask (conn.ExecuteScalarAsync<int64>("SELECT COUNT(*) FROM prodpol.employees;"))
                return! cnt
            }

        member this.GetByIdAsync(key) =
            asyncResult {
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
