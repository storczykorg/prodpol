namespace Storczyk.Prodpol.Core.Services

open System
open System.Collections.Generic
open System.Threading.Tasks
open Dapper
open FSharp.Control
open Microsoft.AspNetCore.Identity
open Npgsql
open Storczyk.Async
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Utils.AsyncIdentityResult

type IPgEmployeeUserStore =
    inherit IUserStore<Employee>
    inherit IUserPasswordStore<Employee>

type PgEmployeeUserStore(dataSource: NpgsqlDataSource) =
    inherit PgEmployeeRepository(dataSource)

    interface IPgEmployeeUserStore with
        member this.CreateAsync(user: Employee, cancellationToken) =
            let result = (this :> IEmployeesRepository).AddAsync(user)
            toIdentityResultAsync result

        member this.DeleteAsync(user: Employee, cancellationToken) =
            let result = (this :> IEmployeesRepository).DeleteAsync(user.Id)
            toIdentityResultAsync result

        member this.Dispose() = dataSource.Dispose()

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
                use! conn = dataSource.OpenConnectionAsync(cancellationToken)

                let! emp =
                    conn.QuerySingleOrDefaultAsync<Employee>(
                        "SELECT * FROM prodpol.employees WHERE LOWER(email) = @normalizedName;",
                        {| normalizedName = normalizedUserName |}
                    )

                if box emp = null then return Employee() else return emp
            }

        member this.GetNormalizedUserNameAsync(user: Employee, cancellationToken) =
            Task.FromResult(
                if box user <> null && not (String.IsNullOrWhiteSpace(user.Email)) then
                    user.Email.ToUpperInvariant()
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
                if not (String.IsNullOrWhiteSpace(user.PasswordHash |> Option.defaultValue null)) then
                    return (user.PasswordHash |> Option.defaultValue "")
                else
                    use! conn = dataSource.OpenConnectionAsync(cancellationToken)

                    let! result =
                        (conn.QuerySingleOrDefaultAsync<string | null>(
                            """
                        SELECT password_hash FROM prodpol.employees
                        """
                        ))

                    return result |> defaultIfNull ""
            }

        member this.HasPasswordAsync(user, cancellationToken) =
            task {
                if not (String.IsNullOrWhiteSpace(user.PasswordHash |> Option.defaultValue null)) then
                    return true
                else
                    use! conn = dataSource.OpenConnectionAsync(cancellationToken)

                    let! result =
                        (conn.QuerySingleOrDefaultAsync<string | null>(
                            """
                        SELECT password_hash FROM prodpol.employees
                        """
                        ))

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
                    """
                    )

                match result with
                | 0 ->
                    do! scope.RollbackAsync()
                    raise (KeyNotFoundException "User not found")
                | 1 ->
                    do! scope.CommitAsync()
                    ()
                | _ ->
                    do! scope.RollbackAsync()
                    raise (InvalidOperationException("SQL returns result code bigger than 1"))

            }
