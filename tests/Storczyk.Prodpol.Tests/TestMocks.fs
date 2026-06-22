module Storczyk.Prodpol.Tests.TestMocks

open System
open Moq
open Microsoft.Extensions.Logging
open FSharp.Control
open System.Threading
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Mvc.ModelBinding.Validation
open Microsoft.AspNetCore.Mvc
open Storczyk.Async
open Storczyk.Prodpol.Core.Utils
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Services

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
        .Returns(fun (_: CancellationToken) -> async { return asyncSeq { for r: EmployeeRole in roles -> r } })
    |> ignore

    repo

let mockRoleRepoGetByIdNotFound () =
    let repo = Mock<IEmployeeRoleRepository>()

    repo
        .Setup(fun m -> m.GetByIdAsync(It.IsAny<string>()))
        .Returns(fun (_: string) -> async { return raise (NotFoundException "Resource not found.") })
    |> ignore

    repo

let mockLogger<'T> () = Mock<ILogger<'T>>()

let mockSnowflakeGenerator (id: int64) =
    let mock = Mock<ISnowflakeGenerator>()
    mock.Setup(fun m -> m.GetSnowflake(It.IsAny<DateTime>())).Returns(id) |> ignore
    mock

let mockEmployeeRepoAddAsync (result: Async<unit>) =
    let mock = Mock<IEmployeesRepository>()
    mock
        .Setup(fun m -> m.AddAsync(It.IsAny<Employee>()))
        .Returns(fun (_: Employee) -> result)
    |> ignore
    mock

let mockEmployeeRepoUpdateAsync (result: Async<unit>) =
    let mock = Mock<IEmployeesRepository>()
    mock
        .Setup(fun m -> m.UpdateAsync(It.IsAny<int64>(), It.IsAny<Employee>()))
        .Returns(fun (_: int64, _: Employee) -> result)
    |> ignore
    mock

let mockEmployeeRepoGetById (emp: Employee) =
    let mock = Mock<IEmployeesRepository>()
    mock
        .Setup(fun m -> m.GetByIdAsync(It.IsAny<int64>()))
        .Returns(fun (_: int64) -> async { return emp })
    |> ignore
    mock

let mockEmployeeRepoGetByIdNotFound () =
    let mock = Mock<IEmployeesRepository>()
    mock
        .Setup(fun m -> m.GetByIdAsync(It.IsAny<int64>()))
        .Returns(fun (_: int64) -> async { return raise (NotFoundException "Resource not found.") })
    |> ignore
    mock

let mockEmployeeReadRepoGetById (emp: EmployeeRead) =
    let mock = Mock<IEmployeesReadRepository>()
    mock
        .Setup(fun m -> m.GetByIdAsync(It.IsAny<int64>()))
        .Returns(fun (_: int64) -> async { return emp })
    |> ignore
    mock

let mockPasswordHasher () =
    let mock = Mock<IPasswordHasher<Employee>>()
    mock
        .Setup(fun m -> m.HashPassword(It.IsAny<Employee>(), It.IsAny<string>()))
        .Returns("hashed-password-test")
    |> ignore
    mock

let setupControllerValidation (controller: ControllerBase) =
    let validatorMock = Mock<IObjectModelValidator>()
    validatorMock
        .Setup(fun v -> v.Validate(
            It.IsAny<ActionContext>(),
            It.IsAny<ValidationStateDictionary>(),
            It.IsAny<string>(),
            It.IsAny<Object>()))
    |> ignore
    controller.ObjectValidator <- validatorMock.Object
