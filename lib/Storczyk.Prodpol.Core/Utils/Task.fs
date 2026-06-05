namespace Storczyk.Prodpol.Core.Utils

open System.Threading.Tasks
open Storczyk.Prodpol.Core.Data

module Task =
    /// Execute a `Task<'T>`-producing function and capture exceptions as `DatabaseError`.
    ///
    /// Converts common cancellation/timeout exceptions into `DatabaseError` variants
    /// and returns `Async<Result<'T, DatabaseError>>` so callers can compose it with
    /// the `AsyncResult` utilities.
    let inline wrapAsync<'T> (x: unit -> Task<'T>) : Async<Result<'T, DatabaseError>> =
        async {
            try
                let! result = x ()

                if obj.ReferenceEquals(result, null) then
                    return Error DatabaseError.NotFound
                else
                    return Ok result
            with e ->
                return DatabaseError.mapError e

        }

    let inline wrap<'T> (x: unit -> 'T) : Async<Result<'T, DatabaseError>> =
        async {
            try
                let result = x ()

                if obj.ReferenceEquals(result, null) then
                    return Error DatabaseError.NotFound
                else
                    return Ok result
            with e ->
                return DatabaseError.mapError e

        }

    /// Execute a `Task<'T option>`-producing function and map `None` to `NotFound`.
    ///
    /// Useful for repository lookups that return optional results.
    let inline wrapOpt (x: unit -> Task<'T option>) : Async<Result<'T, DatabaseError>> =
        async {
            try
                match! x () with
                | None -> return Error DatabaseError.NotFound
                | Some e -> return Ok e
            with e ->
                return DatabaseError.mapError e

        }
(*
Source - https://stackoverflow.com/a/66846981
Posted by Fyodor Soikin, modified by community. See post 'Timeline' for change history
Retrieved 2026-05-16, License - CC BY-SA 4.0
*)
[<AutoOpen>]
module Async =
    /// Execute an `Async<'T>`-producing function and convert exceptions to `DatabaseError`.
    let inline wrap<'T> (x: unit -> Async<'T>) : Async<Result<'T, DatabaseError>> =
        async {
            try
                let! result = x ()
                return Ok result
            with e ->
                return DatabaseError.mapError e
        }

    let inline wrap1<'U, 'T> (x: 'U -> 'T, y: 'U) : Async<Result<'T, DatabaseError>> =
        async {
            try
                let result = x (y)
                return Ok result
            with e ->
                return DatabaseError.mapError e
        }

    let inline wrapAsync (x: Async<'T>) : Async<Result<'T, DatabaseError>> =
        async {
            try
                let! result = x
                return Ok result
            with e ->
                return DatabaseError.mapError e
        }

    let inline wrapTask (x: Task<'T>) : Async<Result<'T, DatabaseError>> =
        async {
            try
                let! result = x
                return Ok result
            with e ->
                return DatabaseError.mapError e
        }

    let inline wrapVoidTask (x: Task) : Async<Result<unit, DatabaseError>> =
        async {
            try
                let! result = x
                return Ok result
            with e ->
                return DatabaseError.mapError e
        }

    let inline wrapValueTask (x: ValueTask<'T>) : Async<Result<'T, DatabaseError>> =
        async {
            try
                let! result = x
                return Ok result
            with e ->
                return DatabaseError.mapError e
        }

    let inline wrapVoidValueTask (x: ValueTask) : Async<Result<unit, DatabaseError>> =
        async {
            try
                let! result = x
                return Ok result
            with e ->
                return DatabaseError.mapError e
        }

    /// Helper to call an async function with an argument and capture errors.
    let inline wrapMap (x: 'b, f: 'b -> Async<'T>) : Async<Result<'T, DatabaseError>> =
        async {
            try
                let! result = f x
                return Ok result
            with e ->
                return DatabaseError.mapError e
        }

    /// Convert a `ValueTask<'T>`-returning function to a `Task<'T>`-returning function.
    let inline toTaskFunc (f: unit -> ValueTask<'T>) : unit -> Task<'T> = fun _ -> task { return! f () }

    /// Convert a `ValueTask<'T>`-returning function to an `Async<'T>`-returning function.
    let inline toAsyncFunc (f: unit -> ValueTask<'T>) : unit -> Async<'T> = fun _ -> async { return! f () }

    /// Map over an `Async` result with a synchronous function.
    let inline map (f: 'a -> 'b) (a: Async<'a>) : Async<'b> =
        async {
            let! x = a
            return f x
        }

module Result =
    /// Unsafe extract: collapse a `Result<'a,'a>` into `'a` by returning either
    /// the success or error value. Use only when the types are intentionally equal.
    let extract a =
        match a with
        | Ok a -> a
        | Error b -> b
