module Storczyk.Prodpol.UnitTest1

open NUnit.Framework

open Moq
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Mvc
open FSharp.Control
open System.Threading
open Storczyk.Async
open Storczyk.Prodpol.Core.Utils
open Storczyk.Prodpol.Controllers.Data
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models

[<SetUp>]
let Setup () = ()

[<Test>]
let Test1 () =
    // Run controller tests inside single discovered test
    // Test: GetAll
    let role = EmployeeRoleRead()
    role.Id <- 1
    role.DisplayName <- "Admin"
    role.RoleName <- "admin"

    let repoMock = Mock<IEmployeeRoleRepository>()

    let readRepoMock = Mock<IEmployeeRoleReadRepository>()
    readRepoMock
        .Setup(fun (m: IEmployeeRoleReadRepository) -> m.GetAllAsync(It.IsAny<CancellationToken>()))
        .Returns(fun (_: CancellationToken) -> async { return asyncSeq { yield role } })
        |> ignore

    let loggerMock = Mock<ILogger<EmployeesRolesController>>()
    let controller = EmployeesRolesController(readRepoMock.Object, repoMock.Object, loggerMock.Object)

    let result = controller.GetAll(CancellationToken.None).Result
    match result with
    | :? OkObjectResult as ok ->
        let value = ok.Value :?> AsyncSeq<EmployeeRole>
        let items = value |> AsyncSeq.toArrayAsync |> Async.RunSynchronously
        Assert.That(items.Length, Is.EqualTo(1))
    | _ -> Assert.Fail("Expected OkObjectResult for GetAll")

    // Test: GetById NotFound
    let repoMock2 = Mock<IEmployeeRoleRepository>()
    repoMock2
        .Setup(fun m -> m.GetByIdAsync(It.IsAny<string>()))
        .Returns(fun (_: string) -> async { return raise (NotFoundException "Resource not found.") })
        |> ignore

    let readRepoMock2 = Mock<IEmployeeRoleReadRepository>()
    let controller2 = EmployeesRolesController(readRepoMock2.Object, repoMock2.Object, loggerMock.Object)
    let result2 = controller2.GetById("missing").Result
    Assert.That(result2, Is.TypeOf<NotFoundResult>(), "Expected NotFoundResult for GetById")

[<Test>]
let GetAll_ReturnsOkWithRoles () =
    let role = EmployeeRoleRead()
    role.Id <- 1
    role.DisplayName <- "Admin"
    role.RoleName <- "admin"

    let repoMock = Mock<IEmployeeRoleRepository>()

    let readRepoMock = Mock<IEmployeeRoleReadRepository>()
    readRepoMock
        .Setup(fun (m: IEmployeeRoleReadRepository) -> m.GetAllAsync(It.IsAny<CancellationToken>()))
        .Returns(fun (_: CancellationToken) -> async { return asyncSeq { yield role } })
        |> ignore

    let loggerMock = Mock<ILogger<EmployeesRolesController>>()
    let controller = EmployeesRolesController(readRepoMock.Object, repoMock.Object, loggerMock.Object)

    let result = controller.GetAll(CancellationToken.None).Result
    match result with
    | :? OkObjectResult as ok ->
        let value = ok.Value :?> AsyncSeq<EmployeeRole>
        let items = value |> AsyncSeq.toArrayAsync |> Async.RunSynchronously
        Assert.That(items.Length, Is.EqualTo(1))
        Assert.That(items.[0].RoleName, Is.EqualTo("admin"))
    | _ -> Assert.Fail("Expected OkObjectResult")

[<Test>]
let GetById_ReturnsNotFound () =
    let repoMock = Mock<IEmployeeRoleRepository>()
    repoMock
        .Setup(fun m -> m.GetByIdAsync(It.IsAny<string>()))
        .Returns(fun (_: string) -> async { return raise (NotFoundException "Resource not found.") })
        |> ignore

    let loggerMock = Mock<ILogger<EmployeesRolesController>>()
    let controller = EmployeesRolesController(Mock<IEmployeeRoleReadRepository>().Object, repoMock.Object, loggerMock.Object)

    let result = controller.GetById("missing").Result
    Assert.That(result, Is.TypeOf<NotFoundResult>(), "Expected NotFoundResult")

[<TestFixture>]
type EmployeesRolesControllerFixture() =
    let createRole id name =
        let r = EmployeeRoleRead()
        r.Id <- id
        r.DisplayName <- name
        r.RoleName <- name.ToLower()
        r

    [<Test>]
    member _.GetAll_ReturnsOk() =
        let role = createRole 1 "Admin"
        let repoMock = Mock<IEmployeeRoleRepository>()

        let readRepoMock = Mock<IEmployeeRoleReadRepository>()
        readRepoMock
            .Setup(fun (m: IEmployeeRoleReadRepository) -> m.GetAllAsync(It.IsAny<CancellationToken>()))
            .Returns(fun (_: CancellationToken) -> async { return asyncSeq { yield role } })
            |> ignore

        let loggerMock = Mock<ILogger<EmployeesRolesController>>()
        let controller = EmployeesRolesController(readRepoMock.Object, repoMock.Object, loggerMock.Object)

        let result = controller.GetAll(CancellationToken.None).Result

        match result with
        | :? OkObjectResult as ok ->
            let value = ok.Value :?> AsyncSeq<EmployeeRole>
            let items = value |> AsyncSeq.toArrayAsync |> Async.RunSynchronously
            Assert.That(items.Length, Is.EqualTo(1))
        | _ -> Assert.Fail("Expected OkObjectResult")

    [<Test>]
    member _.GetById_ReturnsNotFound() =
        let repoMock = Mock<IEmployeeRoleRepository>()
        repoMock
            .Setup(fun m -> m.GetByIdAsync(It.IsAny<string>()))
            .Returns(fun (_: string) -> async { return raise (NotFoundException "Resource not found.") })
            |> ignore

        let loggerMock = Mock<ILogger<EmployeesRolesController>>()
        let controller = EmployeesRolesController(Mock<IEmployeeRoleReadRepository>().Object, repoMock.Object, loggerMock.Object)

        let result = controller.GetById("missing").Result

        match result with
        | :? NotFoundResult -> Assert.Pass()
        | _ -> Assert.Fail("Expected NotFoundResult")
