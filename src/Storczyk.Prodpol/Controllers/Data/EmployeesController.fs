namespace Storczyk.Prodpol.Controllers.Data

open System
open System.Threading
open Microsoft.AspNetCore.JsonPatch.SystemTextJson
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Services
open Storczyk.Prodpol.Core.Utils.AsyncResultCE
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
            [<FromQuery>] ?name: string,
            [<FromQuery>] ?sortingKey: string,
            [<FromQuery>] ?groupId: string
        ) =
        employeesRead.GetAllAsync(token) |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("search")>]
    member this.Search
        (
            token: CancellationToken,
            [<FromServicesAttribute>] employeeSearch: IEmployeeSearchRepository,
            [<FromQuery>] options: EmployeeSearchOption
        ) =
        employeeSearch.SearchAsync(options, token) |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("count")>]
    member this.GetCount(token: CancellationToken) =
        employeesRead.CountAsync(token) |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("{id:long}")>]
    member this.GetById(id: int64) =
        employeesRead.GetByIdAsync(id) |> this.mapAsyncResult

    [<HttpPatch>]
    [<Route("{id:long}")>]
    member this.Update(id: int64, [<FromBody>] update: JsonPatchDocument<Employee>) : Async<ActionResult> =
        asyncResult {
            let! emp = employees.GetByIdAsync(id)
            update.ApplyTo emp
            let! _ = async { return this.ValidateObject emp }
            let! _ = employees.UpdateAsync id emp
            return ()
        }
        |> this.mapAsyncResult

    [<HttpDelete>]
    [<Route("{id:long}")>]
    member this.Delete(id: int64) : Async<ActionResult> =
        employees.DeleteAsync(id) |> this.mapAsyncResult

    [<HttpPost>]
    member this.Create([<FromBody>] entity: EmployeeCreate) =
        let time = DateTime.UtcNow
        let id: int64 = snowflakes.GetSnowflake(time)

        let emp = entity.BuildEmployee(id, time)

        asyncResult {
            let! _ = async { return this.ValidateObject emp }
            let! _ = employees.AddAsync emp
            return id
        }
        |> this.mapAsyncResult
