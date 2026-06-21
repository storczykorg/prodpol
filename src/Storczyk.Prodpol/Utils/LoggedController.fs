namespace Storczyk.Prodpol.Utils

open System.Threading.Tasks
open Microsoft.AspNetCore.JsonPatch.SystemTextJson
open Microsoft.AspNetCore.JsonPatch.SystemTextJson.Exceptions
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc.ModelBinding
open Microsoft.Extensions.Logging
open Storczyk.Async
open Storczyk.Async.AsyncResult

[<AbstractClass>]
type LoggedController() =
    inherit ControllerBase()

    abstract member Logger: ILogger with get

    member this.HandleError(error: DatabaseError) : ActionResult =
        match error with
        | NotFound -> this.NotFound()
        | ConnectionTimeout _ -> this.Problem(statusCode = 408)
        | UnknownException ex ->
            this.Logger.LogError("Unknown error: {0}", ex)
            this.Problem(statusCode = 500)
        | ValidationErrors error ->
            let dict = ModelStateDictionary()
            for e in error do
                dict.TryAddModelError(e.Field, e.Issue) |> ignore
            this.ValidationProblem(dict)
        | e ->
            this.Logger.LogError("Database error: {0}", e)
            this.Problem(statusCode = 500)

    member this.ApplyPatch(patch: JsonPatchDocument<'a>)(obj: 'a): AsyncResult<unit> =
        try
            do patch.ApplyTo obj

            async.Return(Ok())
        with
            | :? JsonPatchException as ex ->
                async.Return(Error (DatabaseError.ValidationErrors [
                    { Field = ex.FailedOperation.path; Issue = ex.Message }
                ]))

    member this.ValidateObject(obj: 'a) : Result<'a, DatabaseError> =
        if this.TryValidateModel(obj) then
            Ok obj
        else
            Error(
                DatabaseError.ValidationErrors(
                    this.ModelState
                    |> Seq.map (fun kv ->
                        { Field = kv.Key
                          Issue = kv.Value.AttemptedValue })
                )
            )

    /// <summary>
    /// Map results from <see cref="AsyncResult"/> to HTTP response
    /// </summary>
    /// <param name="x"></param>
    member this.mapAsyncResult(x: AsyncResult<'a>) : Task<ActionResult> =
        task {
            return! x
            |> map (fun o -> this.Ok(o) :> ActionResult)
            |> (mapError this.HandleError)
            |> extract
        }
