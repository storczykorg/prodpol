namespace Storczyk.Prodpol.Controllers.Data

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Storczyk.Async
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Services
open Storczyk.Prodpol.Core.Utils
open Storczyk.Prodpol.Dat.Forms
open Storczyk.Prodpol.Data.Models
open Storczyk.Prodpol.Utils

[<Authorize>]
[<ApiController>]
[<Route("api/data/customer-spending/")>]
type CustomerSpendingController(
    customerSpendingRead: ICustomerSpendingReadRepository,
    snowflakes: ISnowflakeGenerator,
    logger: ILogger<CustomerSpendingController>
) =
    inherit LoggedController()
    override this.Logger = logger

    [<HttpGet>]
    [<Route("all")>]
    member this.GetAll(
        token: CancellationToken
    ) =
        customerSpendingRead.GetAllAsync(token) |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("count")>]
    member this.GetCount(token: CancellationToken) =
        customerSpendingRead.CountAsync(token) |> this.mapAsyncResult

    [<HttpGet>]
    [<Route("{id:long}")>]
    member this.GetById(id: int64) =
        async {
            let! cs: CustomerSpending = customerSpendingRead.GetByIdAsync(id)
            return cs
        } |> this.mapAsyncResult
