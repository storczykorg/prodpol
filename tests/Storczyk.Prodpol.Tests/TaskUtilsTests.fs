module Storczyk.Prodpol.Tests.TaskUtilsTests

open System
open NUnit.Framework
open System.Threading.Tasks
open Storczyk.Async
open Storczyk.Prodpol.Core.Utils
open Storczyk.Prodpol.Core.Data

[<Test>]
let ``Task.wrap returns value on success`` () =
    let r: int =
        Task.wrapAsync (fun _ -> Task.FromResult 42) |> Async.RunSynchronously
    Assert.That(r, Is.EqualTo(42))

[<Test>]
let ``Task.wrap throws NotFoundException on null`` () =
    Assert.Throws<NotFoundException>(Action(fun () ->
        Task.wrapAsync (fun _ -> Task.FromResult<string>(null))
        |> Async.RunSynchronously
        |> ignore
    )) |> ignore

[<Test>]
let ``Task.wrapOpt throws NotFoundException on None`` () =
    Assert.Throws<NotFoundException>(Action(fun () ->
        Task.wrapOpt (fun _ -> Task.FromResult(None: int option))
        |> Async.RunSynchronously
        |> ignore
    )) |> ignore

[<Test>]
let ``Async.wrap returns value on success`` () =
    let r: int =
        Async.wrap (fun _ -> async { return 7 }) |> Async.RunSynchronously
    Assert.That(r, Is.EqualTo(7))

[<Test>]
let ``Async.wrap lets exceptions propagate`` () =
    Assert.Throws<TimeoutException>(Action(fun () ->
        Async.wrap (fun _ -> async { do raise (TimeoutException()) })
        |> Async.RunSynchronously
    )) |> ignore

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
