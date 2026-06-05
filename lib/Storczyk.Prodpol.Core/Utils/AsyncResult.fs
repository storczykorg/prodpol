namespace Storczyk.Prodpol.Core.Utils

open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Threading.Tasks
open Storczyk.Prodpol.Core.Data

/// Represents an asynchronous computation that may succeed with a value of
/// type `'a` or fail with an error of type `'e`.
///
/// Equivalent to `Async<Result<'a, 'e>>` and used throughout the codebase
/// to compose IO-bound operations that can fail.
type AsyncResult<'a, 'e> = Async<Result<'a, 'e>>
type AsyncResult<'a> = Async<Result<'a, DatabaseError>>

module AsyncResult =

    /// Apply a synchronous mapping function to the success value.
    /// Leaves errors untouched.
    let map (f: 'a -> 'd) (a: AsyncResult<'a, 'e>) : AsyncResult<'d, 'e> =
        async {
            match! a with
            | Ok a -> return Ok(f a)
            | Error e -> return Error e
        }

    /// Map an error value while preserving successful results.
    let mapError (f: 'e -> 'd) (a: AsyncResult<'a, 'e>) : AsyncResult<'a, 'd> =
        async {
            match! a with
            | Ok a -> return Ok a
            | Error e -> return Error(f e)
        }

    /// Bind using a synchronous function that returns a `Result`.
    /// Useful when the next step is pure (non-async) but may fail.
    let bind (f: 'a -> Result<'d, 'e>) (a: AsyncResult<'a, 'e>) : AsyncResult<'d, 'e> =
        async {
            let! x = a
            return Result.bind f x
        }

    /// Bind using an asynchronous function that returns an `AsyncResult`.
    /// Chains async fallible operations.
    let bindAsync (f: 'a -> AsyncResult<'d, 'e>) (a: AsyncResult<'a, 'e>) : AsyncResult<'d, 'e> =
        async {
            match! a with
            | Ok a -> return! f a
            | Error e -> return Error e
        }

    /// Execute an async fallible operation for its side-effects and return
    /// the original successful value on success. If the side-effect fails,
    /// propagate the error.
    let bindIgnore (f: 'a -> AsyncResult<'d, 'e>) (a: AsyncResult<'a, 'e>) : AsyncResult<'a, 'e> =
        async {
            match! a with
            | Ok aa ->
                match! (f aa) with
                | Ok _ -> return Ok aa
                | Error e -> return Error e
            | Error e -> return Error e
        }

    /// Lift a `Result` into `AsyncResult`.
    let Return (x: Result<'a, 'b>) : AsyncResult<'a, 'b> = async.Return x

    /// Extract a successful value from an `AsyncResult<'a,'a>` where the
    /// error type is the same as the success type. This returns an `Async<'a>`
    /// by unwrapping either branch into the same type.
    let extract (a: AsyncResult<'a, 'a>) : Async<'a> =
        async {
            match! a with
            | Ok a -> return a
            | Error b -> return b
        }

    [<Extension>]
    let AsAsyncEnumerable (ty: IEnumerable<'a>) =
        { new IAsyncEnumerable<'a> with
            member this.GetAsyncEnumerator(cancellationToken) =
                let currentEnumerator = ty.GetEnumerator()

                { new IAsyncEnumerator<'a> with
                    member this.MoveNextAsync() =
                        ValueTask.FromResult(currentEnumerator.MoveNext())

                    member this.Current = currentEnumerator.Current
                    member this.DisposeAsync() : ValueTask = ValueTask.CompletedTask

                } }
