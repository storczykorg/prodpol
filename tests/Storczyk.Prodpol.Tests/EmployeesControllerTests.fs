module Storczyk.Prodpol.Tests.EmployeesControllerTests

open System
open NUnit.Framework
open Moq
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Mvc
open FSharp.Control
open System.Threading
open Storczyk.Prodpol.Controllers.Data
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Services
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Utils.AsyncResult

[<Test>]
let ``Search returns Ok with results`` () =
    // Arrange
    let repoMock: Mock<IEmployeeSearchRepository> = Mock<IEmployeeSearchRepository>()

    let item: EmployeeRead = EmployeeRead()
    item.Id <- 42L
    item.FullName <- "Jane Doe"

    let result: EmployeeSearchResult = EmployeeSearchResult()
    result.total <- 1L
    result.results <- [ item ]

    repoMock
        .Setup(fun (m: IEmployeeSearchRepository) ->
            m.SearchAsync(It.IsAny<EmployeeSearchOption>(), It.IsAny<CancellationToken>()))
        .Returns(fun (_: EmployeeSearchOption) (_: CancellationToken) -> async { return Ok(result) })
    |> ignore

    let employeesMock: Mock<IEmployeesRepository> = Mock<IEmployeesRepository>()
    let employeesReadMock: Mock<IEmployeesReadRepository> = Mock<IEmployeesReadRepository>()
    let snowMock = Mock<ISnowflakeGenerator>()

    let loggerMock: Mock<ILogger<EmployeesController>> =
        Mock<ILogger<EmployeesController>>()

    let controller: EmployeesController =
        EmployeesController(employeesMock.Object, employeesReadMock.Object, snowMock.Object, loggerMock.Object)

    // Act
    let actionResult: ActionResult =
        controller.Search(CancellationToken.None, repoMock.Object, EmployeeSearchOption())
        |> Async.RunSynchronously

    // Assert
    match actionResult with
    | :? OkObjectResult as (ok: OkObjectResult) ->
        let v: EmployeeSearchResult = ok.Value :?> EmployeeSearchResult
        Assert.That(v.total, Is.EqualTo(1L))
        let items = v.results |> Seq.toArray
        Assert.That(items.Length, Is.EqualTo(1))
        Assert.That(items.[0].FullName, Is.EqualTo("Jane Doe"))
    | _ -> Assert.Fail("Expected OkObjectResult")

[<Test>]
let ``Search returns NotFound when repository returns NotFound`` () =
    // Arrange
    let repoMock: Mock<IEmployeeSearchRepository> = Mock<IEmployeeSearchRepository>()

    repoMock
        .Setup(fun (m: IEmployeeSearchRepository) ->
            m.SearchAsync(It.IsAny<EmployeeSearchOption>(), It.IsAny<CancellationToken>()))
        .Returns(fun (_: EmployeeSearchOption) (_: CancellationToken) -> async { return Error DatabaseError.NotFound })
    |> ignore

    let employeesMock = Mock<IEmployeesRepository>()
    let employeesReadMock = Mock<IEmployeesReadRepository>()
    let snowMock = Mock<ISnowflakeGenerator>()

    let loggerMock: Mock<ILogger<EmployeesController>> =
        Mock<ILogger<EmployeesController>>()

    let controller =
        EmployeesController(employeesMock.Object, employeesReadMock.Object, snowMock.Object, loggerMock.Object)

    // Act
    let actionResult: ActionResult =
        controller.Search(CancellationToken.None, repoMock.Object, EmployeeSearchOption())
        |> Async.RunSynchronously

    // Assert
    Assert.That(actionResult, Is.TypeOf<NotFoundResult>())
