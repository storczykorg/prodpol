namespace Storczyk.Prodpol.Core.Services

open System
open System.Collections.Generic
open System.Threading.Tasks
open Dapper
open Microsoft.AspNetCore.Identity
open Npgsql
open Storczyk.Async
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Utils.AsyncIdentityResult

type IPgEmployeeUserStore =
    inherit IUserStore<Employee>
    inherit IUserPasswordStore<Employee>
    inherit IUserEmailStore<Employee>
    inherit IUserSecurityStampStore<Employee>

type PgEmployeeUserStore(dataSource: NpgsqlDataSource) =
    inherit PgEmployeeRepository(dataSource)

    interface IPgEmployeeUserStore with
        member this.CreateAsync(user: Employee, cancellationToken) =
            let result = (this :> IEmployeesRepository).AddAsync(user)
            toIdentityResultAsync result

        member this.DeleteAsync(user: Employee, cancellationToken) =
            let result = (this :> IEmployeesRepository).DeleteAsync(user.Id)
            toIdentityResultAsync result

        member this.Dispose() = ()

        member this.FindByIdAsync(userId: string, cancellationToken) : Task<Employee | null> =
            task {
                let mutable id = 0L

                if Int64.TryParse(userId, &id) then
                    try
                        let! o = (this :> IEmployeesRepository).GetByIdAsync(id)
                        return o
                    with :? NotFoundException ->
                        return Employee()
                else
                    return raise (FormatException("userId must be a valid 64-bit integer"))
            }

        member this.FindByNameAsync(normalizedUserName, cancellationToken) : Task<Employee | null> =
            task {
                if String.IsNullOrEmpty(normalizedUserName) then
                    return null
                else
                    use! conn = dataSource.OpenConnectionAsync(cancellationToken)

                    let! emp =
                        conn.QuerySingleOrDefaultAsync<Employee | null>(
                            "SELECT * FROM prodpol.employees WHERE LOWER(email) = @normalizedName;",
                            {| normalizedName = normalizedUserName |}
                        )

                    return emp
            }

        member this.GetNormalizedUserNameAsync(user: Employee, cancellationToken) =
            Task.FromResult(
                if box user <> null && not (String.IsNullOrWhiteSpace(user.Email)) then
                    user.Email.ToLowerInvariant()
                else
                    null
            )

        member this.GetUserIdAsync(user: Employee, cancellationToken) =
            Task.FromResult(if box user <> null then user.Id.ToString() else null)

        member this.GetUserNameAsync(user: Employee, cancellationToken) =
            Task.FromResult(if box user <> null then user.Email else null)

        member this.SetNormalizedUserNameAsync(user: Employee, normalizedName, cancellationToken) = Task.CompletedTask

        member this.SetUserNameAsync(user: Employee, userName, cancellationToken) =
            if box user <> null then
                user.Email <- userName

            Task.CompletedTask

        member this.UpdateAsync(user: Employee, cancellationToken) =
            let result = (this :> IEmployeesRepository).UpdateAsync(user.Id, user)
            toIdentityResultAsync result

        member this.GetPasswordHashAsync(user, cancellationToken) =
            task {
                match user.PasswordHash with
                | Some hash -> return hash
                | None ->
                    use! conn = dataSource.OpenConnectionAsync(cancellationToken)

                    let! result =
                        conn.QuerySingleOrDefaultAsync<string | null>(
                            """
                        SELECT password_hash FROM prodpol.employees
                        WHERE employee_id = (@id)::bigint;
                        """,
                            {| id = user.Id |}
                        )

                    return result |> defaultIfNull ""
            }

        member this.HasPasswordAsync(user, cancellationToken) =
            task {
                match user.PasswordHash with
                | Some _ -> return true
                | None ->
                    use! conn = dataSource.OpenConnectionAsync(cancellationToken)

                    let! result =
                        conn.QuerySingleOrDefaultAsync<string | null>(
                            """
                        SELECT password_hash FROM prodpol.employees
                        WHERE employee_id = (@id)::bigint;
                        """,
                            {| id = user.Id |}
                        )

                    return not (String.IsNullOrWhiteSpace(result))
            }

        member this.SetPasswordHashAsync(user, passwordHash, cancellationToken) =
            task {
                use! conn = dataSource.OpenConnectionAsync(cancellationToken)
                use! scope = conn.BeginTransactionAsync(cancellationToken)

                let! result =
                    conn.ExecuteAsync(
                        """
                    UPDATE prodpol.employees
                    SET password_hash = (@hash::text)
                    where employee_id = (@key)::bigint;
                    """,
                        {| hash = passwordHash; key = user.Id |}
                    )

                match result with
                | 0 ->
                    do! scope.RollbackAsync()
                    raise (KeyNotFoundException "User not found")
                | 1 ->
                    do! scope.CommitAsync()
                    user.PasswordHash <- Some passwordHash
                    ()
                | _ ->
                    do! scope.RollbackAsync()
                    raise (InvalidOperationException("SQL returns result code bigger than 1"))

            }

        member this.GetEmailAsync(user, cancellationToken) =
            Task.FromResult(if box user <> null then user.Email else null)

        member this.SetEmailAsync(user, email, cancellationToken) =
            task {
                use! conn = dataSource.OpenConnectionAsync(cancellationToken)
                use! scope = conn.BeginTransactionAsync(cancellationToken)

                let! result =
                    conn.ExecuteAsync(
                        """
                    UPDATE prodpol.employees
                    SET email = (@email::text)
                    WHERE employee_id = (@key)::bigint;
                    """,
                        {| email = email; key = user.Id |}
                    )

                match result with
                | 0 ->
                    do! scope.RollbackAsync()
                    raise (KeyNotFoundException "User not found")
                | 1 ->
                    do! scope.CommitAsync()

                    if box user <> null then
                        user.Email <- email
                        user.NormalizedEmail <- None

                    ()
                | _ ->
                    do! scope.RollbackAsync()
                    raise (InvalidOperationException("SQL returns result code bigger than 1"))
            }

        member this.GetEmailConfirmedAsync(user, cancellationToken) =
            Task.FromResult(if box user <> null then user.EmailConfirmed else false)

        member this.SetEmailConfirmedAsync(user, confirmed, cancellationToken) =
            task {
                use! conn = dataSource.OpenConnectionAsync(cancellationToken)
                use! scope = conn.BeginTransactionAsync(cancellationToken)

                let! result =
                    conn.ExecuteAsync(
                        """
                    UPDATE prodpol.employees
                    SET email_confirmed = (@confirmed::boolean)
                    WHERE employee_id = (@key)::bigint;
                    """,
                        {| confirmed = confirmed
                           key = user.Id |}
                    )

                match result with
                | 0 ->
                    do! scope.RollbackAsync()
                    raise (KeyNotFoundException "User not found")
                | 1 ->
                    do! scope.CommitAsync()

                    if box user <> null then
                        user.EmailConfirmed <- confirmed

                    ()
                | _ ->
                    do! scope.RollbackAsync()
                    raise (InvalidOperationException("SQL returns result code bigger than 1"))
            }

        member this.GetSecurityStampAsync(user, cancellationToken) =
            task {
                match user.SecurityStamp with
                | Some stamp -> return stamp
                | None ->
                    use! conn = dataSource.OpenConnectionAsync(cancellationToken)

                    let! result =
                        conn.QuerySingleOrDefaultAsync<string | null>(
                            """
                        SELECT security_stamp FROM prodpol.employees
                        WHERE employee_id = (@id)::bigint;
                        """,
                            {| id = user.Id |}
                        )

                    return result |> defaultIfNull ""
            }

        member this.SetSecurityStampAsync(user, stamp, cancellationToken) =
            task {
                use! conn = dataSource.OpenConnectionAsync(cancellationToken)

                let! result =
                    conn.ExecuteAsync(
                        """
                    UPDATE prodpol.employees
                    SET security_stamp = (@stamp::text)
                    WHERE employee_id = (@key)::bigint;
                    """,
                        {| stamp = stamp; key = user.Id |}
                    )

                match result with
                | 0 -> raise (KeyNotFoundException "User not found")
                | 1 ->
                    user.SecurityStamp <- Some stamp
                    ()
                | _ -> raise (InvalidOperationException("SQL returns result code bigger than 1"))
            }

        member this.FindByEmailAsync(normalizedEmail, cancellationToken) : Task<Employee | null> =
            task {
                if String.IsNullOrEmpty(normalizedEmail) then
                    return null
                else
                    use! conn = dataSource.OpenConnectionAsync(cancellationToken)

                    let! emp =
                        conn.QuerySingleOrDefaultAsync<Employee | null>(
                            "SELECT * FROM prodpol.employees WHERE normalized_email = lower(@normalizedEmail);",
                            {| normalizedEmail = normalizedEmail |}
                        )

                    return emp
            }

        member this.GetNormalizedEmailAsync(user, cancellationToken) =
            task {
                match user.NormalizedEmail with
                | Some value -> return value.ToLower()
                | None ->
                    use! conn = dataSource.OpenConnectionAsync(cancellationToken)

                    let! result =
                        conn.QuerySingleOrDefaultAsync<string | null>(
                            """
                        SELECT normalized_email FROM prodpol.employees
                        WHERE employee_id = (@id)::bigint;
                        """,
                            {| id = user.Id |}
                        )

                    return result |> defaultIfNull null
            }

        member this.SetNormalizedEmailAsync(user, normalizedEmail, cancellationToken) =
            if box user <> null then
                user.NormalizedEmail <-
                    if normalizedEmail = null then
                        None
                    else
                        Some normalizedEmail

            Task.CompletedTask
