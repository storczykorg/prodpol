namespace Storczyk.Prodpol.Controllers.Data

open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.JsonPatch.SystemTextJson
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Storczyk.Async
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Utils

[<Authorize>]
[<ApiController>]
[<Route("api/data/customers/roles/")>]
type CustomersRolesController(roles: ICustomerRoleRepository, logger: ILogger<CustomersRolesController>) =
    inherit LoggedController()
    override this.Logger = logger

    [<HttpGet>]
    [<Route("count")>]
    member this.GetCount(token: CancellationToken) =
        roles.CountAsync(token) |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("all")>]
    [<ProducesResponseType(typeof<CustomerRole[]>, 200)>]
    member this.GetAll(token: CancellationToken) =
        async { return! roles.GetAllAsync(token) } |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("{id:long}")>]
    member this.GetById(id: string) =
        roles.GetByIdAsync(id) |> this.mapAsyncResult

    [<HttpPatch>]
    [<Route("{id:long}")>]
    member this.Update(id: string, [<FromBody>] update: JsonPatchDocument<CustomerRole>) : Task<ActionResult> =
        async {
            let! role = roles.GetByIdAsync(id)
            update.ApplyTo role
            this.ValidateObject role |> ignore
            do! roles.UpdateAsync(id, role)
            return role
        }
        |> this.mapAsyncResult

    [<HttpDelete>]
    [<Route("{id:long}")>]
    member this.Delete(id: string) : Task<ActionResult> =
        roles.DeleteAsync(id) |> this.mapAsyncResult

    [<HttpPost>]
    member this.Create([<FromBody>] entity: CustomerRole) =
        async { return this.ValidateObject entity } |> this.mapAsyncResult
