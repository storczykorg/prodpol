namespace Storczyk.Prodpo.Utils
(*
Source - https://stackoverflow.com/a/66846981
Posted by Fyodor Soikin, modified by community. See post 'Timeline' for change history
Retrieved 2026-05-16, License - CC BY-SA 4.0
*)

module Async =
  let map f a = async {
    let! x = a
    return f x
  }

module AsyncResult =
  type AsyncResult<'a, 'e> = Async<Result<'a, 'e>>
  let map (f: 'a -> 'd) (a: AsyncResult<'a, 'e>): AsyncResult<'d, 'e> = async {
    match! a with
    | Ok a -> return Ok (f a)
    | Error e -> return Error e
  }
  let mapError (f: 'e -> 'd) (a: AsyncResult<'a, 'e>): AsyncResult<'a, 'd> = async {
    match! a with
    | Ok a -> return Ok a
    | Error e -> return Error (f e)
  }
  let bind (f: 'a -> Result<'d, 'e>) (a: AsyncResult<'a, 'e>): AsyncResult<'d, 'e> = async {
    let! x = a
    return Result.bind f x
  }
  let bindAsync (f: 'a -> AsyncResult<'d, 'e>) (a: AsyncResult<'a, 'e>): AsyncResult<'d, 'e> = async {
    match! a with
    | Ok a -> return! f a
    | Error e -> return Error e
  }
  let Return (x: Result<'a, 'b>): AsyncResult<'a, 'b> =
    async.Return x
  let extract (a: AsyncResult<'a, 'a>): Async<'a> = async {
    match! a with
    | Ok a -> return a
    | Error b -> return b
  }
  

module Result =
  let extract a =
    match a with
    | Ok a -> a
    | Error b -> b