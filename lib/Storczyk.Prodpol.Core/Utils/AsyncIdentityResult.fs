module Storczyk.Prodpol.Core.Utils.AsyncIdentityResult

open System.Threading.Tasks
open Microsoft.AspNetCore.Identity
open Storczyk.Async

let rec toIdentityErrors (error: DatabaseError) : IdentityError seq =
    seq {
        match error with
        | NotFound ->
            yield IdentityError(Code = "NotFound", Description = "The requested resource was not found.")

        | ConcurrencyViolation ->
            yield IdentityError(Code = "ConcurrencyFailure", Description = "Optimistic concurrency failure, object has been modified.")

        | ConnectionTimeout _ ->
            yield IdentityError(Code = "Timeout", Description = "The database connection timed out.")

        | OperationCancelled _ ->
            yield IdentityError(Code = "Cancelled", Description = "The operation was cancelled.")

        | DatabaseException ex ->
            yield IdentityError(Code = "DatabaseFault", Description = ex.Message)

        | UnknownException ex ->
            yield IdentityError(Code = "UnknownError", Description = ex.Message)

        | ValidationErrors details ->
            for detail in details do
                yield IdentityError(Code = $"ValidationError_{detail.Field}", Description = detail.Issue)

        | OperationErrors nestedErrors ->
            for nested in nestedErrors do
                yield! toIdentityErrors nested
    }
let toIdentityResult (error: DatabaseError) : IdentityResult =
    let errors = toIdentityErrors error |> Seq.toArray
    IdentityResult.Failed(errors)

let toIdentityResultAsync (result: AsyncResult<'a>) : Task<IdentityResult> =
    task {
        match! result with
        | Ok _ -> return IdentityResult.Success
        | Error e -> return toIdentityResult e
    }