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
open Storczyk.Prodpol.Utils

[<Authorize>]
[<ApiController>]
[<Route("api/data/products")>]
type ProductsController
    (
        products: IProductRepository,
        productsRead: IProductReadRepository,
        productSearch: IProductSearchRepository,
        logger: ILogger<ProductsController>
    ) =
    inherit LoggedController()
    override this.Logger = logger

    [<HttpGet>]
    [<Route("all")>]
    [<AllowAnonymous>]
    member this.GetAll(token: CancellationToken) =
        async {
            let! result = productsRead.GetAllAsync(token)
            return result
        } |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("search")>]
    [<AllowAnonymous>]
    member this.Search(token: CancellationToken, [<FromQuery>] options: ProductSearchOption) =
        async {
            return! productSearch.SearchAsync(options, token)
        } |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("count")>]
    [<AllowAnonymous>]
    member this.GetCount(token: CancellationToken) =
        productsRead.CountAsync(token) |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("{id:long}")>]
    [<AllowAnonymous>]
    member this.GetById(id: int64) =
        async {
            let! product: ProductRead = productsRead.GetByIdAsync(id)
            return product
        } |> this.mapAsyncResult

    [<HttpPatch>]
    [<Route("{id:long}")>]
    [<Consumes("text/json")>]
    [<ProducesResponseType(statusCode = 200, Type = typeof<ProductRead>)>]
    member this.Update(id: int64, [<FromBody>] update: JsonPatchDocument<Product>) : Task<ActionResult> =
        async {
            let! product: Product = products.GetByIdAsync(id)

            this.ApplyPatch update product
            this.ValidateObject product |> ignore
            do! products.UpdateAsync(id, product)

            return product
        } |> this.mapAsyncResult

    [<HttpDelete>]
    [<Route("{id:long}")>]
    member this.Delete(id: int64) : Task<ActionResult> =
        products.DeleteAsync(id) |> this.mapAsyncResult
