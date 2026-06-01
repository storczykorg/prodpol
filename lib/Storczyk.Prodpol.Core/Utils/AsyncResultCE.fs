namespace Storczyk.Prodpol.Core.Utils

open System
open System.Threading.Tasks
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Utils.Async

[<AutoOpen>]
module AsyncResultCE =

    type AsyncResult<'a> = Async<Result<'a, DatabaseError>>

    type AsyncResultBuilder() =
        member inline _.Return(x: 'a) : AsyncResult<'a> = async { return Ok x }

        member inline _.ReturnFrom(x: AsyncResult<'a>) = x

        member inline _.Zero() : AsyncResult<unit> = async { return Ok() }

        member inline _.Delay(f: unit -> AsyncResult<'a>) : AsyncResult<'a> = async.Delay(f)
        //member inline _.Delay(f: unit -> Async<'a>): AsyncResult<'a> =
        //    wrap f
        //member inline _.Delay(f: unit -> ValueTask<'a>): AsyncResult<'a> =
        //    wrap (toAsyncFunc f)

        member inline _.Bind(a: AsyncResult<'a>, f: 'a -> AsyncResult<'b>) : AsyncResult<'b> =
            async {
                let! r = a

                match r with
                | Ok v -> return! f v
                | Error e -> return Error e
            }

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

        member _.TryWith(body: unit -> AsyncResult<'a>, handler: exn -> AsyncResult<'a>) : AsyncResult<'a> =
            async {
                try
                    return! body ()
                with ex ->
                    return! handler ex
            }

        member _.TryFinally(body: unit -> AsyncResult<'a>, compensation: unit -> unit) : AsyncResult<'a> =
            async {
                try
                    return! body ()
                finally
                    compensation ()
            }

        member _.Using(resource: IDisposable, f: IDisposable -> AsyncResult<'a>) : AsyncResult<'a> =
            async {
                try
                    return! f resource
                finally
                    if not (isNull (box resource)) then
                        resource.Dispose()
            }

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

    let asyncResult: AsyncResultBuilder = AsyncResultBuilder()
