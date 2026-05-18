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
