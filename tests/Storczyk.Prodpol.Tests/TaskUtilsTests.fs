module Storczyk.Prodpol.Tests.TaskUtilsTests

open System
open NUnit.Framework
open System.Threading.Tasks
open Storczyk.Prodpol.Core.Utils
open Storczyk.Prodpol.Core.Data

[<Test>]
let ``Task.wrap returns Ok on success`` () =
    let r: Result<int, DatabaseError> =
        Task.wrapAsync (fun _ -> Task.FromResult 42) |> Async.RunSynchronously

    match r with
    | Ok v -> Assert.That(v, Is.EqualTo(42))
    | Error e -> Assert.Fail(sprintf "Expected Ok but got Error %A" e)

[<Test>]
let ``Task.wrap maps OperationCanceledException to OperationCancelled error`` () =
    let t: Result<int, DatabaseError> =
        Task.wrapAsync (fun _ ->
            Task.Run<int>(fun () ->
                raise (OperationCanceledException())
                0))
        |> Async.RunSynchronously

    match t with
    | Error DatabaseError.OperationCancelled -> Assert.Pass()
    | _ -> Assert.Fail("Expected OperationCancelled error")

[<Test>]
let ``Task.wrap maps TimeoutException to ConnectionTimeout error`` () =
    let t: Result<int, DatabaseError> =
        Task.wrapAsync (fun _ ->
            Task.Run<int>(fun () ->
                raise (TimeoutException())
                0))
        |> Async.RunSynchronously

    match t with
    | Error DatabaseError.ConnectionTimeout -> Assert.Pass()
    | _ -> Assert.Fail("Expected ConnectionTimeout error")

[<Test>]
let ``Task.wrapOpt maps None to NotFound`` () =
    let r: Result<int, DatabaseError> =
        Task.wrapOpt (fun _ -> Task.FromResult(None: int option))
        |> Async.RunSynchronously

    match r with
    | Error DatabaseError.NotFound -> Assert.Pass()
    | _ -> Assert.Fail("Expected NotFound error")

[<Test>]
let ``Async.wrap returns Ok on success`` () =
    let r: Result<int, DatabaseError> =
        Async.wrap (fun _ -> async { return 7 }) |> Async.RunSynchronously

    match r with
    | Ok v -> Assert.That(v, Is.EqualTo(7))
    | Error e -> Assert.Fail(sprintf "Expected Ok but got Error %A" e)

[<Test>]
let ``Async.wrap maps TimeoutException to ConnectionTimeout`` () =
    let r: Result<unit, DatabaseError> =
        Async.wrap (fun _ -> async { do raise (TimeoutException()) })
        |> Async.RunSynchronously

    match r with
    | Error DatabaseError.ConnectionTimeout -> Assert.Pass()
    | _ -> Assert.Fail("Expected ConnectionTimeout error")

[<Test>]
let ``toTaskFunc and toAsyncFunc convert ValueTask functions`` () =
    let tf = Async.toTaskFunc (fun _ -> ValueTask<int>(13))
    let t: Task<int> = tf ()
    Assert.That(t.Result, Is.EqualTo(13))

    let af = Async.toAsyncFunc (fun _ -> ValueTask<int>(21))
    let a: Async<int> = af ()
    let v = Async.RunSynchronously a
    Assert.That(v, Is.EqualTo(21))

[<Test>]
let ``Async.map maps result`` () =
    let a = Async.map ((+) 1) (async { return 2 }) |> Async.RunSynchronously
    Assert.That(a, Is.EqualTo(3))

[<Test>]
let ``Result.extract returns value from Ok or Error when types match`` () =
    let a = Result.extract (Ok 5)
    Assert.That(a, Is.EqualTo(5))

    let b = Result.extract (Error 5)
    Assert.That(b, Is.EqualTo(5))
