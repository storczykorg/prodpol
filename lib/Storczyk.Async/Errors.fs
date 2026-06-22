namespace Storczyk.Async

open System

type ValidationErrorDetail = { Field: string; Issue: string }

type NotFoundException(message: string, inner: exn) =
    inherit Exception(message, inner)
    new (message: string) = NotFoundException(message, null)
    new () = NotFoundException("Resource not found.", null)

type ValidationErrorException(errors: ValidationErrorDetail seq, message: string, inner: exn) =
    inherit Exception(message, inner)
    new (errors: ValidationErrorDetail seq) = ValidationErrorException(errors, "Validation failed", null)
    new (errors: ValidationErrorDetail seq, message: string) = ValidationErrorException(errors, message, null)
    member this.Errors = errors
