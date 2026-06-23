namespace Storczyk.Prodpol.Utils

open System.Threading.Tasks
open Microsoft.AspNetCore.JsonPatch.SystemTextJson
open Microsoft.AspNetCore.JsonPatch.SystemTextJson.Exceptions
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc.ModelBinding
open Microsoft.Extensions.Logging
open Storczyk.Async
open Storczyk.Prodpol.Core.Utils

[<AbstractClass>]
type LoggedController() =
    inherit ControllerBase()

    abstract member Logger: ILogger with get

    member this.HandleError(ex: System.Exception) : ActionResult =
        match ex with
        | :? NotFoundException -> this.NotFound()
        | :? System.Collections.Generic.KeyNotFoundException -> this.NotFound()
        | :? ValidationErrorException as vex ->
            let dict = ModelStateDictionary()

            for e in vex.Errors do
                dict.TryAddModelError(e.Field, e.Issue) |> ignore

            BadRequestObjectResult(dict) :> ActionResult
        | :? System.TimeoutException -> this.Problem(statusCode = 408)
        | :? System.OperationCanceledException -> this.Problem(statusCode = 499)
        | :? ReferentialIntegrityException as ex ->
            this.Logger.LogWarning(ex, "Referential integrity violation")
            this.Problem(statusCode = 409, title = "Cannot delete resource", detail = ex.Message)
        | _ ->
            this.Logger.LogError(ex, "Unhandled error")
            this.Problem(statusCode = 500)

    member this.ApplyPatch (patch: JsonPatchDocument<'a>) (obj: 'a) : unit =
        try
            do patch.ApplyTo obj
        with :? JsonPatchException as ex ->
            raise (
                ValidationErrorException(
                    [ { Field = ex.FailedOperation.path
                        Issue = ex.Message } ]
                )
            )

    member this.ValidateObject(obj: 'a) : 'a =
        if this.TryValidateModel(obj) then
            obj
        else
            let errors =
                this.ModelState
                |> Seq.map (fun kv ->
                    { Field = kv.Key
                      Issue = kv.Value.AttemptedValue })

            raise (ValidationErrorException(errors))

    member this.mapAsyncResult(x: Async<'a>) : Task<ActionResult> =
        task {
            try
                let! result = x
                return this.Ok(result) :> ActionResult
            with ex ->
                return this.HandleError(ex) :> ActionResult
        }
