namespace Storczyk.Prodpol.Core.Utils

module AsyncResult =
    type AsyncResult<'a, 'e> = Async<Result<'a, 'e>>

    let map (f: 'a -> 'd) (a: AsyncResult<'a, 'e>) : AsyncResult<'d, 'e> =
        async {
            match! a with
            | Ok a -> return Ok(f a)
            | Error e -> return Error e
        }

    let mapError (f: 'e -> 'd) (a: AsyncResult<'a, 'e>) : AsyncResult<'a, 'd> =
        async {
            match! a with
            | Ok a -> return Ok a
            | Error e -> return Error(f e)
        }

    let bind (f: 'a -> Result<'d, 'e>) (a: AsyncResult<'a, 'e>) : AsyncResult<'d, 'e> =
        async {
            let! x = a
            return Result.bind f x
        }

    let bindAsync (f: 'a -> AsyncResult<'d, 'e>) (a: AsyncResult<'a, 'e>) : AsyncResult<'d, 'e> =
        async {
            match! a with
            | Ok a -> return! f a
            | Error e -> return Error e
        }

    /// Execute mapping operation and ignore its results
    let bindIgnore (f: 'a -> AsyncResult<'d, 'e>) (a: AsyncResult<'a, 'e>) : AsyncResult<'a, 'e> =
        async {
            match! a with
            | Ok aa ->
                match! (f aa) with
                | Ok _ -> return Ok aa
                | Error e -> return Error e
            | Error e -> return Error e
        }

    let Return (x: Result<'a, 'b>) : AsyncResult<'a, 'b> = async.Return x

    let extract (a: AsyncResult<'a, 'a>) : Async<'a> =
        async {
            match! a with
            | Ok a -> return a
            | Error b -> return b
        }
