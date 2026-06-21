namespace Storczyk.Prodpol.Controllers.Data

open System.Runtime.InteropServices
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.JsonPatch.SystemTextJson
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Storczyk.Async
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Services
open Storczyk.Prodpol.Core.Utils
open Storczyk.Prodpol.Utils

[<ApiController>]
[<Route("api/data/employees/photos/")>]
type EmployeePhotosController(photos: IRepository<int64, EmployeePhoto>,
                              logger: ILogger<EmployeePhotosController>) =
    inherit LoggedController()

    [<HttpGet>]
    [<Route("all")>]
    member this.GetAll() =
        failwith "TODO"
        ""

    [<HttpGet; Route("{id:long}")>]
    [<Produces("image/png", "image/webp")>]
    member this.GetById(id: int64,
                        [<Optional; DefaultParameterValue(64)>]
                        w: int) =
        async {
           match! asyncResult {
                let! result : EmployeePhoto = photos.GetByIdAsync(id)

                return this.File(
                    result.Payload,
                    result.MimeType)
            } with
           | Ok r -> return r :> IActionResult
           | Error (DatabaseError.NotFound) -> return StatusCodeResult(StatusCodes.Status404NotFound) :> IActionResult
           | e ->
               let! x = this.mapAsyncResult(async{return e})
               return x :> IActionResult
        }

    [<HttpPatch>]
    [<Route("{id:long}")>]
    member this.Update() =
        failwith "TODO"
        ""

    [<HttpDelete>]
    [<Route("{id:long}")>]
    member this.Delete(id: int64) =
        failwith "TODO"
        ""

    override this.Logger = logger
