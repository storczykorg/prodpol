module Storczyk.Prodpol.Tests.EmployeesRolesControllerTests

open System
open NUnit.Framework
open Moq
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Mvc
open FSharp.Control
open System.Threading
open Storczyk.Prodpol.Controllers.Data
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Utils.AsyncResult

[<Test>]
let ``GetAll returns Ok with roles`` () =
    // Arrange
    let role: EmployeeRole = EmployeeRole()
    role.Id <- 1
    role.DisplayName <- "Admin"
    role.RoleName <- "admin"

    let repoMock: Mock<IEmployeeRoleRepository> = Mock<IEmployeeRoleRepository>()

    repoMock
        .Setup(fun m -> m.GetAllAsync(It.IsAny<CancellationToken>()))
        .Returns(fun (_: CancellationToken) -> async { return Ok(asyncSeq { yield role }) })
    |> ignore

    let loggerMock = Mock<ILogger<EmployeesRolesController>>()

    let controller = EmployeesRolesController(repoMock.Object, loggerMock.Object)

    // Act
    let result = controller.GetAll(CancellationToken.None) |> Async.RunSynchronously

    // Assert
    match result with
    | :? OkObjectResult as ok ->
        let value = ok.Value :?> AsyncSeq<EmployeeRole>
        let items = value |> AsyncSeq.toArrayAsync |> Async.RunSynchronously
        Assert.That(items.Length, Is.EqualTo(1))
        Assert.That(items.[0].RoleName, Is.EqualTo("admin"))
    | _ -> Assert.That(result, Is.TypeOf<OkObjectResult>(), "Expected OkObjectResult")

[<Test>]
let ``GetById returns NotFound when repository returns NotFound`` () =
    // Arrange
    let repoMock = Mock<IEmployeeRoleRepository>()

    repoMock
        .Setup(fun m -> m.GetByIdAsync(It.IsAny<string>()))
        .Returns(fun (_: string) -> async { return Error DatabaseError.NotFound })
    |> ignore

    let loggerMock = Mock<ILogger<EmployeesRolesController>>()
    let controller = EmployeesRolesController(repoMock.Object, loggerMock.Object)

    // Act
    let result = controller.GetById("missing") |> Async.RunSynchronously

    // Assert
    Assert.That(result, Is.TypeOf<NotFoundResult>(), "Expected NotFoundResult")
