module Storczyk.Prodpol.Tests.TestMocks

open Moq
open Microsoft.Extensions.Logging
open FSharp.Control
open System.Threading
open Storczyk.Async
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Data

let createRole id name =
    let r = EmployeeRole()
    r.Id <- id
    r.DisplayName <- name
    r.RoleName <- name.ToLower()
    r

let mockRoleRepoGetAll (roles: seq<EmployeeRole>) =
    let repo = Mock<IEmployeeRoleRepository>()

    repo
        .Setup(fun m -> m.GetAllAsync(It.IsAny<CancellationToken>()))
        .Returns(fun (_: CancellationToken) -> async { return Ok(asyncSeq { for r: EmployeeRole in roles -> r }) })
    |> ignore

    repo

let mockRoleRepoGetByIdNotFound () =
    let repo = Mock<IEmployeeRoleRepository>()

    repo
        .Setup(fun m -> m.GetByIdAsync(It.IsAny<string>()))
        .Returns(fun (_: string) -> async { return Error DatabaseError.NotFound })
    |> ignore

    repo

let mockLogger<'T> () = Mock<ILogger<'T>>()
