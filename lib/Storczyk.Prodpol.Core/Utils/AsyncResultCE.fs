namespace Storczyk.Prodpol.Core.Utils

open System
open System.Threading.Tasks
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Utils.Async

[<AutoOpen>]
module AsyncResultCE =

    /// Compute expression alias for `Async<Result<_, DatabaseError>>` specialised
    /// to this project's `DatabaseError` type. Use `asyncResult { ... }` to
    /// compose async fallible workflows with familiar computation-expression
    /// syntax.
    type AsyncResult<'a> = Async<Result<'a, DatabaseError>>

    type AsyncResultBuilder() =
        /// Return a successful value into the computation expression.
        member inline _.Return(x: 'a) : AsyncResult<'a> = async { return Ok x }

        /// Return an existing `AsyncResult` directly.
        member inline _.ReturnFrom(x: AsyncResult<'a>) = x

        /// Zero for expression: represents `Ok ()`.
        member inline _.Zero() : AsyncResult<unit> = async { return Ok() }

        /// Delay evaluation of the body.
        member inline _.Delay(f: unit -> AsyncResult<'a>) : AsyncResult<'a> = async.Delay(f)
        //member inline _.Delay(f: unit -> Async<'a>): AsyncResult<'a> =
        //    wrap f
        //member inline _.Delay(f: unit -> ValueTask<'a>): AsyncResult<'a> =
        //    wrap (toAsyncFunc f)

        /// Bind an `AsyncResult` producing the next computation.
        member inline _.Bind(a: AsyncResult<'a>, f: 'a -> AsyncResult<'b>) : AsyncResult<'b> =
            async {
                let! r = a

                match r with
                | Ok v -> return! f v
                | Error e -> return Error e
            }

        /// Support binding `ValueTask` results inside the expression.
        member inline _.Bind(a: ValueTask<'a>, f: 'a -> AsyncResult<'b>) : AsyncResult<'b> =
            async {
                let! r =
                    wrap (fun _ ->
                        async {
                            let! z = a
                            return! f z
                        })

                match r with
                | Ok q -> return q
                | Error e -> return Error e
            }

        /// TryWith to handle exceptions inside the computation expression.
        member _.TryWith(body: unit -> AsyncResult<'a>, handler: exn -> AsyncResult<'a>) : AsyncResult<'a> =
            async {
                try
                    return! body ()
                with ex ->
                    return! handler ex
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
        member _.Using(resource: IDisposable, f: IDisposable -> AsyncResult<'a>) : AsyncResult<'a> =
            async {
                try
                    return! f resource
                finally
                    if not (isNull (box resource)) then
                        resource.Dispose()
            }

        /// For helper to iterate a sequence with async fallible body.
        member _.For(sequence: seq<'a>, body: 'a -> AsyncResult<unit>) : AsyncResult<unit> =
            async {
                use enum = sequence.GetEnumerator()

                let rec loop () =
                    async {
                        if enum.MoveNext() then
                            let! res = body enum.Current

                            match res with
                            | Ok() -> return! loop ()
                            | Error e -> return Error e
                        else
                            return Ok()
                    }

                return! loop ()
            }

    /// Computation expression instance to use in code: `asyncResult { ... }`.
    let asyncResult: AsyncResultBuilder = AsyncResultBuilder()
