module Storczyk.Prodpol.Core.Utils.AsyncIdentityResult

open System.Threading.Tasks
open Microsoft.AspNetCore.Identity
open Storczyk.Async

let toIdentityErrors (ex: System.Exception) : IdentityError seq =
    seq {
        match ex with
        | :? NotFoundException ->
            yield IdentityError(Code = "NotFound", Description = "The requested resource was not found.")

        | :? ValidationErrorException as vex ->
            for e in vex.Errors do
                yield IdentityError(Code = $"ValidationError_{e.Field}", Description = e.Issue)

        | :? System.InvalidOperationException ->
            yield
                IdentityError(
                    Code = "ConcurrencyFailure",
                    Description = "Optimistic concurrency failure, object has been modified."
                )

        | :? System.TimeoutException ->
            yield IdentityError(Code = "Timeout", Description = "The database connection timed out.")

        | :? System.OperationCanceledException ->
            yield IdentityError(Code = "Cancelled", Description = "The operation was cancelled.")

        | _ -> yield IdentityError(Code = "UnknownError", Description = ex.Message)
    }

let toIdentityResult (ex: System.Exception) : IdentityResult =
    let errors = toIdentityErrors ex |> Seq.toArray
    IdentityResult.Failed(errors)

let toIdentityResultAsync (action: Async<unit>) : Task<IdentityResult> =
    task {
        try
            do! action
            return IdentityResult.Success
        with ex ->
            return toIdentityResult ex
    }
