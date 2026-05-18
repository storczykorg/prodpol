namespace Storczyk.Prodpol.Controllers.Data

open System
open System.Threading
open Microsoft.AspNetCore.JsonPatch.SystemTextJson
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Services
open Storczyk.Prodpol.Core.Utils.AsyncResult
open Storczyk.Prodpol.Dat.Forms
open Storczyk.Prodpol.Utils

[<ApiController>]
[<Route("api/data/employees/")>]
type EmployeesController
    (employees: IEmployeesRepository, snowflakes: ISnowflakeGenerator, logger: ILogger<EmployeesController>) =
    inherit LoggedController()
    override this.Logger = logger

    [<HttpGet>]
    [<Route("all")>]
    member this.GetAll(token: CancellationToken) =
        employees.GetAllAsync(token) |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("{id:long}")>]
    member this.GetById(id: int64) =
        employees.GetByIdAsync(id) |> this.mapAsyncResult

    [<HttpPatch>]
    [<Route("{id:long}")>]
    member this.Update(id: int64, [<FromBody>] update: JsonPatchDocument<Employee>) : Async<ActionResult> =
        employees.GetByIdAsync(id)
        |> (bind (fun emp ->
            update.ApplyTo emp
            this.ValidateObject emp))
        |> bindIgnore (employees.UpdateAsync id)
        |> this.mapAsyncResult

    [<HttpDelete>]
    [<Route("{id:long}")>]
    member this.Delete(id: int64) : Async<ActionResult> =
        employees.DeleteAsync(id) |> this.mapAsyncResult

    [<HttpPost>]
    member this.Create([<FromBody>] entity: EmployeeCreate) =
        let time = DateTime.UtcNow
        let id: int64 = snowflakes.GetSnowflake(time)

        let emp = entity.GetEmployee(id, time)

        Return(this.ValidateObject emp)
        |> (bindAsync employees.AddAsync)
        |> (map (fun _ -> id))
        |> this.mapAsyncResult
