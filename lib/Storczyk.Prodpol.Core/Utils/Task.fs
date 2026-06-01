namespace Storczyk.Prodpol.Core.Utils

open System
open System.Threading.Tasks
open Storczyk.Prodpol.Core.Data

module Task =
    let inline wrap (x: unit -> Task<'T>) : Async<Result<'T, DatabaseError>> =
        async {
            try
                let! result = x ()
                return Ok result
            with
            | :? OperationCanceledException -> return Error DatabaseError.OperationCancelled
            | :? TimeoutException -> return Error DatabaseError.ConnectionTimeout

        }

    let inline wrapOpt (x: unit -> Task<'T option>) : Async<Result<'T, DatabaseError>> =
        async {
            try
                match! x () with
                | None -> return Error DatabaseError.NotFound
                | Some e -> return Ok e
            with
            | :? OperationCanceledException -> return Error DatabaseError.OperationCancelled
            | :? TimeoutException -> return Error DatabaseError.ConnectionTimeout

        }
(*
Source - https://stackoverflow.com/a/66846981
Posted by Fyodor Soikin, modified by community. See post 'Timeline' for change history
Retrieved 2026-05-16, License - CC BY-SA 4.0
*)

module Async =
    let inline wrap (x: unit -> Async<'T>) : Async<Result<'T, DatabaseError>> =
        async {
            try
                let! result = x ()
                return Ok result
            with
            | :? OperationCanceledException -> return Error DatabaseError.OperationCancelled
            | :? TimeoutException -> return Error DatabaseError.ConnectionTimeout
        }

    let inline wrapMap (x: 'b, f: 'b -> Async<'T>) : Async<Result<'T, DatabaseError>> =
        async {
            try
                let! result = f x
                return Ok result
            with
            | :? OperationCanceledException -> return Error DatabaseError.OperationCancelled
            | :? TimeoutException -> return Error DatabaseError.ConnectionTimeout
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
