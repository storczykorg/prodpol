namespace Storczyk.Async

open System
open System.Collections.Generic
open System.Data

type ValidationErrorDetail = { Field: string; Issue: string }

type DatabaseError =
    | NotFound
    | ValidationErrors of ValidationErrorDetail seq
    | OperationErrors of DatabaseError seq
    | ConcurrencyViolation
    | ConnectionTimeout of TimeoutException
    | OperationCancelled of OperationCanceledException
    | DatabaseException of DataException
    | UnknownException of Exception

module DatabaseError =
    let mapError (e: Exception) : Result<'a, DatabaseError> =
        match e with
        | :? OperationCanceledException as t -> Error(DatabaseError.OperationCancelled(t))
        | :? TimeoutException as t -> Error(DatabaseError.ConnectionTimeout t)
        | :? AggregateException as ae ->
            match ae.InnerExceptions |> Seq.tryHead with
            | Some(:? OperationCanceledException as t) -> Error(DatabaseError.OperationCancelled t)
            | Some(:? TimeoutException as t) -> Error(DatabaseError.ConnectionTimeout t)
            | _ -> Error(DatabaseError.UnknownException(ae))
        | _ -> Error(DatabaseError.UnknownException(e))

    let toResult (f: 'a -> 'b) (a: 'a) : Result<'b, DatabaseError> =
        try
            let result = f a

            if obj.ReferenceEquals(result, null) then
                Error DatabaseError.NotFound
            else
                Ok result
        with e ->
            mapError e

    let extractException (e: DatabaseError) =
        match e with
        | OperationCancelled t -> t :> System.Exception
        | ConnectionTimeout t -> t
        | DatabaseException ex -> ex
        | UnknownException ex -> ex
        | NotFound -> (KeyNotFoundException("Resource not found."))
        | ConcurrencyViolation -> (InvalidOperationException("Concurrency violation occurred."))
        | ValidationErrors errors ->
            let msg = errors |> Seq.map (fun v -> $"{v.Field}: {v.Issue}") |> String.concat "; "
            (ArgumentException($"Validation failed: {msg}"))
        | OperationErrors _ -> (AggregateException("Multiple database operations failed.", [||]))
    let throwError (e: DatabaseError) =
        raise (extractException e)
