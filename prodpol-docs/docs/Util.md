# Utilities

This page documents small helper functions used across the codebase, primarily
in `lib/Storczyk.Prodpol.Core/Utils`.

## Task/Async wrappers

- `Task.wrap` — run a `Task<'T>`-producing function and convert common
  exceptions into `DatabaseError` variants, returning `Async<Result<'T,DatabaseError>>`.

- `Task.wrapOpt` — run a `Task<'T option>`-producing function and map `None`
  to `DatabaseError.NotFound`.

- `Async.wrap` — run an `Async<'T>`-producing function and capture exceptions
  as `DatabaseError`.

- `Async.wrapMap` — call an `Async` function with an argument and capture errors.

These helpers are used by repository implementations and other IO-bound code to
normalize error handling and make them composable with `AsyncResult` utilities.

## Converters

- `toTaskFunc` — convert `unit -> ValueTask<'T>` into `unit -> Task<'T>`.
- `toAsyncFunc` — convert `unit -> ValueTask<'T>` into `unit -> Async<'T>`.

Use these when interoperating with APIs that expose `ValueTask`.

## Small helpers

- `Async.map` — map a synchronous function over an `Async<'T>`
- `Result.extract` — collapse a `Result<'a,'a>` into `'a` by returning either
  branch's value. Use only when the types are intentionally equal.

## Location

Files:
- `lib/Storczyk.Prodpol.Core/Utils/Task.fs`
- `lib/Storczyk.Prodpol.Core/Utils/Async.fs` (module content inside Task.fs)

This page: `prodpol-docs/docs/Util.md`.
