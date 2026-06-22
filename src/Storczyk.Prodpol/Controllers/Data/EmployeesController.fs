namespace Storczyk.Prodpol.Controllers.Data

open System
open System.Linq
open System.Threading
open System.Threading.Tasks
open System.Transactions
open FSharp.Control
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.JsonPatch.SystemTextJson
open Microsoft.AspNetCore.JsonPatch.SystemTextJson.Exceptions
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Storczyk.Async
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Services
open Storczyk.Prodpol.Core.Utils
open Storczyk.Prodpol.Dat.Forms
open Storczyk.Prodpol.Utils

[<ApiController>]
[<Route("api/data/employees/")>]
type EmployeesController
    (
        employees: IEmployeesRepository,
        employeesRead: IEmployeesReadRepository,
        snowflakes: ISnowflakeGenerator,
        logger: ILogger<EmployeesController>
    ) =
    inherit LoggedController()
    override this.Logger = logger

    [<HttpGet>]
    [<Route("all")>]
    member this.GetAll
        (
            token: CancellationToken,
            [<FromQuery>] ?roleName: string
        ) =
        asyncResult {
            let! result: AsyncSeq<EmployeeRead> = employeesRead.GetAllAsync(token)
            if (String.IsNullOrEmpty(roleName |> Option.toObj)) then
                return result
            else
                return result.Where(fun x -> String.Compare(x.RoleName |> Option.defaultValue "", roleName |> Option.defaultValue "") = 0)
        } |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("search")>]
    member this.Search
        (
            token: CancellationToken,
            [<FromServices>] employeeSearch: IEmployeeSearchRepository,
            [<FromQuery>] options: EmployeeSearchOption
        ) =
        asyncResult {
           return! employeeSearch.SearchAsync(options, token)
        } |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("count")>]
    member this.GetCount(token: CancellationToken) =
        employeesRead.CountAsync(token) |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("{id:long}")>]
    member this.GetById(id: int64) =
        asyncResult {
            let! emp: EmployeeRead = employeesRead.GetByIdAsync(id)
            return emp
        } |> this.mapAsyncResult

    [<HttpPatch; Route("{id:long}")>]
    [<Consumes("text/json")>]
    [<ProducesResponseType(statusCode = 200, Type = typeof<EmployeeRead>)>]
    member this.Update(id: int64, [<FromBody>] update: JsonPatchDocument<Employee>) : Task<ActionResult> =
        (asyncResult {
            let! emp: Employee = employees.GetByIdAsync(id)

            do! this.ApplyPatch update emp
            do! this.ValidateObject emp
            do! employees.UpdateAsync id emp


            return emp


        }: AsyncResult<Employee>)
        |> this.mapAsyncResult

    [<HttpDelete>]
    [<Route("{id:long}")>]
    member this.Delete(id: int64) : Task<ActionResult> =
        employees.DeleteAsync(id) |> this.mapAsyncResult

    [<HttpPost; ProducesResponseType(typeof<EmployeeRead>, 200)>]
    member this.Create([<FromBody>] entity: EmployeeCreate,
                       passwordHasher: IPasswordHasher<Employee>) =
        let time = DateTime.UtcNow
        let id: int64 = snowflakes.GetSnowflake(time)

        let emp = entity.BuildEmployee(id, time)

        if (entity.passwordNotEmpty()) then
            emp.PasswordHash <-
                Some(passwordHasher
                         .HashPassword(emp, (entity.Password |> defaultIfNull ""))
                         )

        (asyncResult {
            do! this.ValidateObject emp |> Result.map ignore
            do! employees.AddAsync emp
            return! employeesRead.GetByIdAsync(id)
        }: AsyncResult<EmployeeRead>)
        |> this.mapAsyncResult
