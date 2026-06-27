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
                    (employee_id, name_first, name_last, email, phone_number, password_hash, security_stamp)
                    VALUES
                        (@newKey, @nameFirst,
                         @nameLast, @email,
                         @phoneNumber, @passwordHash, @securityStamp);"

                let! result: int =
                    wrapTask (
                        conn.ExecuteAsync(
                            sql,
                            {| newKey = entity.Id
                               nameFirst = entity.NameFirst
                               nameLast = entity.NameLast
                               email = entity.Email
                               phoneNumber = entity.PhoneNumber
                               passwordHash = entity.PasswordHash |> Option.defaultValue null
                               securityStamp = entity.SecurityStamp |> Option.defaultValue null |}
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
            let isForeignKeyError (ex: exn): bool =
                match ex with
                | :? NpgsqlException as e when e.SqlState = "23502" -> true
                | _ -> false
            async {
                try
                    let! ct = Async.CancellationToken
                    use! conn = dataSource.OpenConnectionAsync(ct)
                    use! scope = conn.BeginTransactionAsync(ct)
                    let! result =
                            conn.ExecuteAsync(
                                // language=postgresql
                                "DELETE FROM prodpol.employees
                                WHERE employee_id = (@id);",
                                {| id = key |}
                            )
                    match result with
                    | 0 ->
                        do! scope.RollbackAsync()
                        return raise (NotFoundException "Resource not found.")
                    | 1 ->
                        do! scope.CommitAsync(ct)
                        return ()
                    | _ ->
                        do! scope.RollbackAsync()
                        return raise (
                            InvalidOperationException
                                "Expected affected rows to be a value between 0 and 1. Evaluate query"
                        )
                with
                | :? PostgresException as ex when isForeignKeyError(ex) ->
                    raise (ReferentialIntegrityException(ex.TableName, ex.ColumnName, ex))
                | :? AggregateException as exs when (exs.InnerExceptions |> Seq.forall(isForeignKeyError)) ->
                    raise (ReferentialIntegrityException("Muliple foreign resource depend on the object", "", AggregateException(
                                                             "",
                                                             seq {
                                                                 for item in exs.InnerExceptions do
                                                                     let ex = (item :?> PostgresException)
                                                                     yield (ReferentialIntegrityException (
                                                                                ex . TableName, ex . ColumnName,
                                                                                ex)) :> exn;
                                                             })))

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
                            password_hash = @passwordHash,
                            security_stamp = @securityStamp,
                            email_confirmed = @emailConfirmed,
                            role_id = @roleId
                        WHERE employee_id = @oldId;",
                        {| oldId = key
                           newKey = entity.Id
                           nameFirst = entity.NameFirst
                           nameLast = entity.NameLast
                           email = entity.Email
                           phoneNumber = entity.PhoneNumber
                           passwordHash = entity.PasswordHash |> Option.defaultValue null
                           securityStamp = entity.SecurityStamp |> Option.defaultValue null
                           emailConfirmed = entity.EmailConfirmed
                           roleId = entity.RoleId |> Option.toNullable |}
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
                            """SELECT *
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
