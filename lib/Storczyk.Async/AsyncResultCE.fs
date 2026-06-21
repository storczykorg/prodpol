namespace Storczyk.Async

open System
open System.Threading.Tasks
open Storczyk.Async.AsyncResult
open Storczyk.Async.DatabaseError

[<AutoOpen>]
module AsyncResultCE =

    /// Compute expression alias for `Async<Result<_, DatabaseError>>` specialised
    /// to this project's `DatabaseError` type. Use `asyncResult { ... }` to
    /// compose async fallible workflows with familiar computation-expression
    /// syntax.
    type AsyncResult<'a> = Async<Result<'a, DatabaseError>>

    type AsyncResultBuilder() =
        /// Return a successful value into the computation expression.
        member inline _.Return<'a>(x: 'a) : AsyncResult<'a> = async { return Ok x }

        member inline _.Return<'a>(x: Async<'a>) : AsyncResult<'a> =
            async {
                let! r = x
                return Ok r
            }

        /// Return an existing `AsyncResult` directly.
        member inline _.ReturnFrom<'a>(x: AsyncResult<'a>) : AsyncResult<'a> = x
        member inline _.ReturnFrom(x: Result<'a, DatabaseError>) = async { return x }

        /// Zero for expression: represents `Ok ()`.
        member inline _.Zero() : AsyncResult<unit> = async { return Ok() }

        /// Delay evaluation of the body.
        member inline _.Delay<'a>(f: unit -> AsyncResult<'a>) : AsyncResult<'a> = f ()

        member inline _.Bind<'b>(a: Task, f: unit -> AsyncResult<'b>) : AsyncResult<'b> = (bindAsync f) (wrapVoidTask a)

        member inline _.Bind(a: AsyncResult<'a>, f: unit -> AsyncResult<'a>) : AsyncResult<'a> =
            (bindIgnore (fun _ -> f ())) a

        member inline _.Bind(a: Result<'a, DatabaseError>, f: unit -> AsyncResult<'a>) : AsyncResult<'a> =
            (bindIgnore (fun _ -> f ())) (async { return a })

        member inline _.Bind<'a, 'b>(a: AsyncResult<'a>, f: 'a -> AsyncResult<'b>) : AsyncResult<'b> = bindAsync f a


        member inline _.Bind<'a, 'b>(a: ValueTask<'a>, f: 'a -> AsyncResult<'b>) : AsyncResult<'b> =
            let x: AsyncResult<'a> = wrapValueTask a
            bindAsync f x

        member inline _.Bind<'a, 'b>(a: Result<'a, DatabaseError>, f: 'a -> AsyncResult<'b>) : AsyncResult<'b> =
            match a with
            | Error e -> async { return Error e }
            | Ok o ->
                try
                    f o
                with
                | e -> async { return DatabaseError.mapError e }

        member inline _.Bind<'a, 'b>(a: Task<'a>, f: 'a -> AsyncResult<'b>) : AsyncResult<'b> =
            let x: AsyncResult<'a> = wrapTask a
            bindAsync f x


        member inline _.Bind<'a, 'b>(a: Async<'a>, f: 'a -> AsyncResult<'b>) : AsyncResult<'b> =
            let x: AsyncResult<'a> = wrapAsync a
            bindAsync f x

        /// TryWith to handle exceptions inside the computation expression.
        // member _.TryWith(body: unit -> AsyncResult<'a>, handler: exn -> AsyncResult<'a>) : AsyncResult<'a> =
        //     async {
        //         try
        //             return! body ()
        //         with ex ->
        //             return! handler ex
        //     }
        member _.TryWith(body: AsyncResult<'a>, handler: exn -> AsyncResult<'a>) : AsyncResult<'a> =
            async {
                match! body with
                | Ok ok -> return Ok(ok)
                | Error e -> return! handler(extractException e)
            }

        /// TryFinally to run compensation after the body, even on exceptions.
        member _.TryFinally(body: unit -> AsyncResult<'a>, compensation: unit -> unit) : AsyncResult<'a> =
            async {
                try
                    return! body ()
                finally
                    compensation ()
            }

        /// Using helper for `use` pattern; disposes resource after use.
        member _.Using<'T, 'U when 'T :> IAsyncDisposable>
            (asyncResource: 'T, f: 'T -> AsyncResult<'U>)
            : AsyncResult<'U> =
            async {
                try
                    let! result = (AsyncResult.bindAsync f) (async { return Ok(asyncResource) })
                    do! asyncResource.DisposeAsync()
                    return result
                with e ->
                    do! asyncResource.DisposeAsync().AsAsync()
                    return DatabaseError.mapError e
            }


        /// For helper to iterate a sequence with async fallible body.
        member _.For(sequence: seq<'a>, body: 'a -> AsyncResult<unit>) : AsyncResult<unit> =
            async {
                use enum = sequence.GetEnumerator()

                let rec loop () =
                    async {
                        if enum.MoveNext() then
                            let! res = body (enum.Current)

                            match res with
                            | Ok() -> return! loop ()
                            | Error e -> return Error e
                        else
                            return Ok()
                    }

                return! loop ()
            }

        member _.Combine<'a, 'b>(a: AsyncResult<'a>, b: AsyncResult<'b>) : AsyncResult<'b> =
            async {
                match! a with
                | Ok _ -> return! b
                | Error e -> return Error e
            }


    /// Computation expression instance to use in code: `asyncResult { ... }`.
    let asyncResult: AsyncResultBuilder = AsyncResultBuilder()