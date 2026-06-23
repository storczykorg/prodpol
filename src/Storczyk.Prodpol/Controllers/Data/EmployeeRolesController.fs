namespace Storczyk.Prodpol.Controllers.Data

open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.JsonPatch.SystemTextJson
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Storczyk.Async
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Utils

[<ApiController>]
[<Route("api/data/employees/roles/")>]
type EmployeesRolesController
    (readRoles: IEmployeeRoleReadRepository, roles: IEmployeeRoleRepository, logger: ILogger<EmployeesRolesController>)
    =
    inherit LoggedController()
    override this.Logger = logger

    [<HttpGet>]
    [<Route("count")>]
    member this.GetCount(token: CancellationToken) =
        roles.CountAsync(token) |> this.mapAsyncResult

    [<HttpGet; Route("all")>]
    [<ProducesResponseType(typeof<EmployeeRoleRead[]>, 200)>]
    member this.GetAll(token: CancellationToken) =
        async { return! readRoles.GetAllAsync(token) } |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("{id:long}")>]
    member this.GetById(id: string) =
        roles.GetByIdAsync(id) |> this.mapAsyncResult

    [<HttpPatch>]
    [<Route("{id:long}")>]
    member this.Update(id: string, [<FromBody>] update: JsonPatchDocument<EmployeeRole>) : Task<ActionResult> =
        async {
            let! emp = roles.GetByIdAsync(id)
            update.ApplyTo emp
            this.ValidateObject emp |> ignore
            do! roles.UpdateAsync(id, emp)
            return emp
        }
        |> this.mapAsyncResult

    [<HttpDelete>]
    [<Route("{id:long}")>]
    member this.Delete(id: string) : Task<ActionResult> =
        roles.DeleteAsync(id) |> this.mapAsyncResult

    [<HttpPost>]
    member this.Create([<FromBody>] entity: EmployeeRole) =
        async { return this.ValidateObject entity } |> this.mapAsyncResult
