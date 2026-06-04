# AsyncResult

`AsyncResult<'T,'E>` is a type alias for `Async<Result<'T,'E>>` used to represent
asynchronous computations that may succeed with a value (`Ok`) or fail with an
error (`Error`).

## Motivation

When composing I/O-bound operations that can fail (database calls, HTTP requests,
etc.), using `AsyncResult` lets you keep both the asynchronous and error-handling
aspects explicit and composable.

## Core functions

- `map` — transform a successful value: `AsyncResult<'a,'e> -> AsyncResult<'b,'e>`
- `mapError` — transform an error value
- `bind` — bind with a synchronous `Result`-returning function
- `bindAsync` — bind with an async `AsyncResult`-returning function
- `bindIgnore` — run an async side-effect and keep the original value on success
- `Return` — lift a `Result` into `AsyncResult`
- `extract` — unwrap `AsyncResult<'a,'a>` into `Async<'a>` by collapsing both branches

## Computation Expression (CE)

This project also provides a computation expression specialized for database
workflows: `asyncResult { ... }` (see `AsyncResultCE.fs`). It wraps
`Async<Result<_, DatabaseError>>` and exposes the usual CE helpers so you can
write sequential async code that short-circuits on the first `Error`.

Core CE features:

- `Return`, `ReturnFrom`, `Zero`, `Delay` — lifecycle helpers
- `Bind` — chain `AsyncResult` or `ValueTask` producers
- `TryWith`, `TryFinally`, `Using`, `For` — exception/resource helpers

Example with the CE:

```fsharp
open Storczyk.Prodpol.Core.Utils.AsyncResultCE

let handleRequest id =
    asyncResult {
        let! user = fetchUser id
        do! logAccess user
        let validated = validate user
        match validated with
        | Ok u -> return u
        | Error e -> return! AsyncResult.Return (Error e)
    }
```

See `lib/Storczyk.Prodpol.Core/Utils/AsyncResultCE.fs` for implementation details.

## Examples

```fsharp
open Storczyk.Prodpol.Core.Utils.AsyncResult

let fetchUser id : AsyncResult<User, DatabaseError> =
    // ...
    async { return Ok user }

let validate user : Result<User, ValidationError> =
    // ...

let pipeline id : AsyncResult<User, DatabaseError> =
    fetchUser id
    |> AsyncResult.bind validate
    |> AsyncResult.map (fun u -> { u with Name = u.Name.Trim() })
```
