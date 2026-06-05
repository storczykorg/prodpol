namespace Storczyk.Prodpol.Core.Data

open System
open System.Data

type ValidationErrorDetail = { Field: string; Issue: string }

type DatabaseError =
    | NotFound
    | ValidationErrors of ValidationErrorDetail seq
    | OperationErrors of DatabaseError seq
    | ConcurrencyViolation
    | ConnectionTimeout
    | OperationCancelled
    | DatabaseException of DataException
    | UnknownException of Exception

module DatabaseError =
    let mapError (e: Exception) : Result<'a, DatabaseError> =
        match e with
        | :? OperationCanceledException -> Error DatabaseError.OperationCancelled
        | :? TimeoutException -> Error DatabaseError.ConnectionTimeout
        | :? AggregateException as ae ->
            match ae.InnerExceptions |> Seq.tryHead with
            | Some(:? OperationCanceledException) -> Error DatabaseError.OperationCancelled
            | Some(:? TimeoutException) -> Error DatabaseError.ConnectionTimeout
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
