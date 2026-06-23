namespace Storczyk.Prodpol.Core.Services

open System
open System.Linq
open System.Threading
open LinqToDB
open LinqToDB.Async
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Utils
open Storczyk.Prodpol.Data.Services.Identity

[<RegisterAsTransient(typeof<IEmployeeSearchRepository>)>]
type PgEmployeeSearchRepository(db: IIdentityDatabase) =

    let buildFilteredQuery (fullName: string) (email: string) (phoneNumber: string) (roleNames: string array) =
        let q =
            query {
                for emp in db.EmployeeRead do
                    where (fullName = "" || emp.NormalizedName.Contains(fullName))
                    where (email = "" || emp.Email.Contains(email))
                    where (phoneNumber = "" || emp.PhoneNumber.Contains(phoneNumber))
                    select emp
            }

        if roleNames.Length = 0 then
            q
        else
            let roleNameFilter =
                roleNames
                |> Array.map (fun n ->
                    let safe = n.Replace("'", "''")
                    $"\"role_name\" = '{safe}'")
                |> String.concat " OR "

            q.Where(fun e -> Sql.Expr<bool>($"({roleNameFilter})"))

    let buildOrderedQuery (q: IQueryable<EmployeeRead>) (key: EmployeeOrderKeys) (asc: bool) =
        match key, asc with
        | EmployeeOrderKeys.FullName, true ->
            q.OrderBy(fun e -> e.NormalizedName).ThenBy(fun e -> e.RoleName).ThenBy(fun e -> e.Id)
        | EmployeeOrderKeys.FullName, false ->
            q
                .OrderByDescending(fun e -> e.NormalizedName)
                .ThenByDescending(fun e -> e.RoleName)
                .ThenByDescending(fun e -> e.Id)
        | EmployeeOrderKeys.Email, true ->
            q.OrderBy(fun e -> e.NormalizedEmail).ThenBy(fun e -> e.RoleName).ThenBy(fun e -> e.Id)
        | EmployeeOrderKeys.Email, false ->
            q
                .OrderByDescending(fun e -> e.NormalizedEmail)
                .ThenByDescending(fun e -> e.RoleName)
                .ThenByDescending(fun e -> e.Id)
        | EmployeeOrderKeys.PhoneNumber, true ->
            q.OrderBy(fun e -> e.PhoneNumber).ThenBy(fun e -> e.RoleName).ThenBy(fun e -> e.Id)
        | EmployeeOrderKeys.PhoneNumber, false ->
            q
                .OrderByDescending(fun e -> e.PhoneNumber)
                .ThenByDescending(fun e -> e.RoleName)
                .ThenByDescending(fun e -> e.Id)
        | EmployeeOrderKeys.RoleName, true -> q.OrderBy(fun e -> e.RoleName).ThenBy(fun e -> e.Id)
        | EmployeeOrderKeys.RoleName, false -> q.OrderByDescending(fun e -> e.RoleName).ThenByDescending(fun e -> e.Id)
        | _, true -> q.OrderBy(fun e -> e.Id).ThenBy(fun e -> e.Id)
        | _, false -> q.OrderByDescending(fun e -> e.Id).ThenByDescending(fun e -> e.Id)

    let countResults (q: IQueryable<EmployeeRead>) (token: CancellationToken) =
        async {
            let! count = q.LongCountAsync(token) |> Async.AwaitTask
            return int count
        }

    interface IEmployeeSearchRepository with
        member this.CountSearchAsync(options: EmployeeSearchOption, token) =
            async {
                let fullName = options.fullName |> Option.defaultValue ""
                let email = options.email |> Option.defaultValue ""
                let phoneNumber = options.phoneNumber |> Option.defaultValue ""
                let roleNames = options.roleNames |> Option.defaultValue [||]

                return! countResults (buildFilteredQuery fullName email phoneNumber roleNames) token
            }

        member this.SearchAsync(options: EmployeeSearchOption, token) =
            async {
                let fullName = options.fullName |> Option.defaultValue ""
                let email = options.email |> Option.defaultValue ""
                let phoneNumber = options.phoneNumber |> Option.defaultValue ""
                let roleNames = options.roleNames |> Option.defaultValue [||]
                let orderKey = options.orderBy |> Option.defaultValue EmployeeOrderKeys.EmployeeId

                let filtered = buildFilteredQuery fullName email phoneNumber roleNames
                let ordered = buildOrderedQuery filtered orderKey options.asc

                let! page =
                    ordered.Skip(options.skip).Take(options.limit).ToArrayAsync(token)
                    |> Async.AwaitTask

                let! total = countResults filtered token

                let nextCursor =
                    if page.Length > 0 && int64 options.skip + page.LongLength < total then
                        Some(page.LongLength + int64 options.skip)
                    else
                        None

                return EmployeeSearchResult(results = page, nextCursor = nextCursor, total = total)
            }
