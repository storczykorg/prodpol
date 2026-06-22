namespace Storczyk.Async

open System.Threading.Tasks

module Task =
    let inline wrapAsync<'T> (x: unit -> Task<'T>) : Async<'T> =
        async {
            let! result = x ()

            if obj.ReferenceEquals(result, null) then
                return raise (NotFoundException "Resource not found.")
            else
                return result
        }

    let inline wrap<'T> (x: unit -> 'T) : Async<'T> =
        async {
            let result = x ()

            if obj.ReferenceEquals(result, null) then
                return raise (NotFoundException "Resource not found.")
            else
                return result
        }

    let inline wrapOpt (x: unit -> Task<'T option>) : Async<'T> =
        async {
            match! x () with
            | None -> return raise (NotFoundException "Resource not found.")
            | Some e -> return e
        }

[<AutoOpen>]
module Async =
    let inline wrap<'T> (x: unit -> Async<'T>) : Async<'T> =
        async {
            let! result = x ()
            return result
        }

    let inline wrap1<'U, 'T> (x: 'U -> 'T, y: 'U) : Async<'T> =
        async {
            let result = x (y)
            return result
        }

    let inline wrapAsync (x: Async<'T>) : Async<'T> = x

    let inline wrapTask (x: Task<'T>) : Async<'T> =
        async {
            let! result = x
            return result
        }

    let inline wrapVoidTask (x: Task) : Async<unit> =
        async {
            do! x
            return ()
        }

    let inline wrapValueTask (x: ValueTask<'T>) : Async<'T> =
        async {
            let! result = x
            return result
        }

    let inline wrapVoidValueTask (x: ValueTask) : Async<unit> =
        async {
            do! x
            return ()
        }

    let inline wrapMap (x: 'b, f: 'b -> Async<'T>) : Async<'T> =
        async {
            let! result = f x
            return result
        }

    let inline toTaskFunc (f: unit -> ValueTask<'T>) : unit -> Task<'T> = fun _ -> task { return! f () }

    let inline toAsyncFunc (f: unit -> ValueTask<'T>) : unit -> Async<'T> = fun _ -> async { return! f () }

    let inline map (f: 'a -> 'b) (a: Async<'a>) : Async<'b> =
        async {
            let! x = a
            return f x
        }

module Result =
    let extract a =
        match a with
        | Ok a -> a
        | Error b -> b
