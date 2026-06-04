namespace Storczyk.Prodpol.Utils

open System
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Utils.AsyncResult

[<AbstractClass>]
type LoggedController() =
    inherit ControllerBase()

    abstract member Logger: ILogger with get

    member this.HandleError(error: DatabaseError) : ActionResult =
        match error with
        | NotFound -> this.NotFound()
        | ConnectionTimeout -> this.Problem(statusCode = 408)
        | UnknownException ex ->
            this.Logger.LogError("Unknown error: {0}", ex)
            this.Problem(statusCode = 500)
        | ValidationErrors error -> this.ValidationProblem()
        | e ->
            this.Logger.LogError("Database error: {0}", e)
            this.Problem(statusCode = 500)

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

    member this.mapAsyncResult(x: AsyncResult<'a, DatabaseError>) : Async<ActionResult> =
        x
        |> map (fun o -> this.Ok(o) :> ActionResult)
        |> (mapError this.HandleError)
        |> extract
