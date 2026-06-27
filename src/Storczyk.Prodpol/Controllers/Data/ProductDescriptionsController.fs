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
[<Route("api/data/products/descriptions")>]
type ProductDescriptionsController
    (
        descriptions: IProductDescriptionRepository,
        logger: ILogger<ProductDescriptionsController>
    ) =
    inherit LoggedController()
    override this.Logger = logger

    [<HttpGet>]
    [<Route("all")>]
    [<AllowAnonymous>]
    member this.GetAll(token: CancellationToken) =
        async {
            let! result = descriptions.GetAllAsync(token)
            return result
        } |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("count")>]
    [<AllowAnonymous>]
    member this.GetCount(token: CancellationToken) =
        descriptions.CountAsync(token) |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("{productId:long}")>]
    [<AllowAnonymous>]
    member this.GetById(productId: int64, [<FromQuery>] ?languageCode: string) =
        async {
            let! desc: ProductDescription = descriptions.GetByIdAsync(productId, languageCode)
            return desc
        } |> this.mapAsyncResult

    [<HttpPatch>]
    [<Route("{productId:long}")>]
    [<Consumes("text/json")>]
    [<ProducesResponseType(statusCode = 200, Type = typeof<ProductDescription>)>]
    member this.Update(productId: int64, [<FromBody>] update: JsonPatchDocument<ProductDescription>, [<FromQuery>] ?languageCode: string) : Task<ActionResult> =
        async {
            let! desc: ProductDescription = descriptions.GetByIdAsync(productId, languageCode)

            this.ApplyPatch update desc
            this.ValidateObject desc |> ignore
            do! descriptions.UpdateAsync((productId, languageCode), desc)

            return desc
        } |> this.mapAsyncResult

    [<HttpDelete>]
    [<Route("{productId:long}")>]
    member this.Delete(productId: int64, [<FromQuery>] ?languageCode: string) : Task<ActionResult> =
        descriptions.DeleteAsync(productId, languageCode) |> this.mapAsyncResult
