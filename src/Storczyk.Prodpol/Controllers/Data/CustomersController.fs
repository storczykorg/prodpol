namespace Storczyk.Prodpol.Controllers.Data

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.JsonPatch.SystemTextJson
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Storczyk.Async
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Services
open Storczyk.Prodpol.Core.Utils
open Storczyk.Prodpol.Dat.Forms
open Storczyk.Prodpol.Utils

[<Authorize>]
[<ApiController>]
[<Route("api/data/customers/")>]
type CustomersController
    (
        customers: ICustomerRepository,
        customersRead: ICustomerReadRepository,
        customerSearch: ICustomerSearchRepository,
        logger: ILogger<CustomersController>
    ) =
    inherit LoggedController()
    override this.Logger = logger

    [<HttpGet>]
    [<Route("all")>]
    member this.GetAll(token: CancellationToken) =
        async {
            let! result = customersRead.GetAllAsync(token)
            return result
        } |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("search")>]
    member this.Search(token: CancellationToken, [<FromQuery>] options: CustomerSearchOption) =
        async {
            return! customerSearch.SearchAsync(options, token)
        } |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("count")>]
    member this.GetCount(token: CancellationToken) =
        customersRead.CountAsync(token) |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("{id:long}")>]
    member this.GetById(id: int64) =
        async {
            let! customer: CustomerRead = customersRead.GetByIdAsync(id)
            return customer
        } |> this.mapAsyncResult

    [<HttpPatch>]
    [<Route("{id:long}")>]
    [<Consumes("text/json")>]
    [<ProducesResponseType(statusCode = 200, Type = typeof<CustomerRead>)>]
    member this.Update(id: int64, [<FromBody>] update: JsonPatchDocument<Customer>) : Task<ActionResult> =
        async {
            let! customer: Customer = customers.GetByIdAsync(id)

            this.ApplyPatch update customer
            this.ValidateObject customer |> ignore
            do! customers.UpdateAsync(id, customer)

            return customer
        } |> this.mapAsyncResult

    [<HttpDelete>]
    [<Route("{id:long}")>]
    member this.Delete(id: int64) : Task<ActionResult> =
        customers.DeleteAsync(id) |> this.mapAsyncResult
