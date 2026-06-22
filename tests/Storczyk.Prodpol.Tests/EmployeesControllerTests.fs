module Storczyk.Prodpol.Tests.EmployeesControllerTests

open System
open System.Text.Json
open NUnit.Framework
open Moq
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc.ModelBinding.Validation
open Microsoft.AspNetCore.JsonPatch.SystemTextJson
open FSharp.Control
open System.Threading
open Storczyk.Async
open Storczyk.Prodpol.Controllers.Data
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Services
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Dat.Forms

let private setupControllerValidation (controller: ControllerBase) =
    let validatorMock = Mock<IObjectModelValidator>()
    validatorMock
        .Setup(fun (v: IObjectModelValidator) -> v.Validate(
            It.IsAny<ActionContext>(),
            It.IsAny<ValidationStateDictionary>(),
            It.IsAny<string>(),
            It.IsAny<Object>()))
    |> ignore
    controller.ObjectValidator <- validatorMock.Object

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
        controller.Search(CancellationToken.None, repoMock.Object, EmployeeSearchOption()).Result

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
        controller.Search(CancellationToken.None, repoMock.Object, EmployeeSearchOption()).Result

    // Assert
    Assert.That(actionResult, Is.TypeOf<NotFoundResult>())

[<Test>]
let ``Create returns Ok with EmployeeRead`` () =
    // Arrange
    let entity = EmployeeCreate()
    entity.Email <- "test@example.com"
    entity.NameFirst <- "Test"
    entity.NameLast <- "User"
    entity.PhoneNumber <- "+48123456789"

    let snowflakeId = 100L

    let employeesRepoMock = Mock<IEmployeesRepository>()
    employeesRepoMock
        .Setup(fun (m: IEmployeesRepository) -> m.AddAsync(It.IsAny<Employee>()))
        .Returns(fun (_: Employee) -> async { return Ok () })
    |> ignore

    let expectedEmp = EmployeeRead()
    expectedEmp.Id <- snowflakeId
    expectedEmp.Email <- "test@example.com"
    expectedEmp.NameFirst <- "Test"
    expectedEmp.NameLast <- "User"
    expectedEmp.PhoneNumber <- "+48123456789"

    let employeesReadRepoMock = Mock<IEmployeesReadRepository>()
    employeesReadRepoMock
        .Setup(fun (m: IEmployeesReadRepository) -> m.GetByIdAsync(It.IsAny<int64>()))
        .Returns(fun (_: int64) -> async { return Ok expectedEmp })
    |> ignore

    let snowMock = Mock<ISnowflakeGenerator>()
    snowMock.Setup(fun (m: ISnowflakeGenerator) -> m.GetSnowflake(It.IsAny<DateTime>())).Returns(snowflakeId) |> ignore

    let loggerMock = Mock<ILogger<EmployeesController>>()

    let controller =
        EmployeesController(employeesRepoMock.Object, employeesReadRepoMock.Object, snowMock.Object, loggerMock.Object)

    setupControllerValidation controller

    let passwordHasherMock = Mock<Microsoft.AspNetCore.Identity.IPasswordHasher<Employee>>()
    passwordHasherMock
        .Setup(fun (m: Microsoft.AspNetCore.Identity.IPasswordHasher<Employee>) ->
            m.HashPassword(It.IsAny<Employee>(), It.IsAny<string>()))
        .Returns("hashed-password-test")
    |> ignore

    // Act
    let actionResult: ActionResult =
        controller.Create(entity, passwordHasherMock.Object).Result

    // Assert
    match actionResult with
    | :? OkObjectResult as (ok: OkObjectResult) ->
        let v: EmployeeRead = ok.Value :?> EmployeeRead
        Assert.That(v.Id, Is.EqualTo(snowflakeId))
        Assert.That(v.Email, Is.EqualTo("test@example.com"))
        Assert.That(v.NameFirst, Is.EqualTo("Test"))
        Assert.That(v.NameLast, Is.EqualTo("User"))
    | _ -> Assert.Fail("Expected OkObjectResult")

[<Test>]
let ``Create returns BadRequest when email already exists`` () =
    // Arrange
    let entity = EmployeeCreate()
    entity.Email <- "duplicate@example.com"
    entity.NameFirst <- "Test"
    entity.NameLast <- "User"
    entity.PhoneNumber <- "+48123456789"

    let snowflakeId = 100L
    let errors = System.Collections.Generic.List<ValidationErrorDetail>()
    errors.Add({ Field = "Email"; Issue = "Email is already present" })

    let employeesRepoMock = Mock<IEmployeesRepository>()
    employeesRepoMock
        .Setup(fun (m: IEmployeesRepository) -> m.AddAsync(It.IsAny<Employee>()))
        .Returns(fun (_: Employee) -> async { return Error (DatabaseError.ValidationErrors(errors)) })
    |> ignore

    let employeesReadRepoMock = Mock<IEmployeesReadRepository>()
    let snowMock = Mock<ISnowflakeGenerator>()
    snowMock.Setup(fun (m: ISnowflakeGenerator) -> m.GetSnowflake(It.IsAny<DateTime>())).Returns(snowflakeId) |> ignore

    let loggerMock = Mock<ILogger<EmployeesController>>()

    let controller =
        EmployeesController(employeesRepoMock.Object, employeesReadRepoMock.Object, snowMock.Object, loggerMock.Object)

    setupControllerValidation controller

    let passwordHasherMock = Mock<Microsoft.AspNetCore.Identity.IPasswordHasher<Employee>>()
    passwordHasherMock
        .Setup(fun (m: Microsoft.AspNetCore.Identity.IPasswordHasher<Employee>) ->
            m.HashPassword(It.IsAny<Employee>(), It.IsAny<string>()))
        .Returns("hashed-password-test")
    |> ignore

    // Act
    let actionResult: ActionResult =
        controller.Create(entity, passwordHasherMock.Object).Result

    // Assert
    match actionResult with
    | :? ObjectResult as (obj: ObjectResult) ->
        Assert.That(obj.StatusCode, Is.EqualTo(Nullable 400))
    | _ -> Assert.Fail("Expected 400 ObjectResult")

[<Test>]
let ``Update returns Ok with patched EmployeeRead`` () =
    // Arrange
    let employeeId = 1L

    let existingEmp = Employee()
    existingEmp.Id <- employeeId
    existingEmp.NameFirst <- "OldName"
    existingEmp.NameLast <- "User"
    existingEmp.Email <- "old@example.com"
    existingEmp.PhoneNumber <- "+48123456789"

    let employeesRepoMock = Mock<IEmployeesRepository>()
    employeesRepoMock
        .Setup(fun (m: IEmployeesRepository) -> m.GetByIdAsync(It.IsAny<int64>()))
        .Returns(fun (_: int64) -> async { return Ok existingEmp })
    |> ignore

    let employeesReadRepoMock = Mock<IEmployeesReadRepository>()
    let snowMock = Mock<ISnowflakeGenerator>()
    let loggerMock = Mock<ILogger<EmployeesController>>()

    let controller =
        EmployeesController(employeesRepoMock.Object, employeesReadRepoMock.Object, snowMock.Object, loggerMock.Object)

    setupControllerValidation controller

    let patchJson = """[{"op":"replace","path":"/nameFirst","value":"NewName"}]"""
    let patch = JsonSerializer.Deserialize<JsonPatchDocument<Employee>>(patchJson)

    employeesRepoMock
        .Setup(fun (m: IEmployeesRepository) -> m.UpdateAsync(It.IsAny<int64>())(It.IsAny<Employee>()))
        .Returns(fun (_key: int64) (_emp: Employee) -> async { return Ok () })
    |> ignore

    // Act
    let actionResult: ActionResult =
        controller.Update(employeeId, patch).Result

    // Assert
    match actionResult with
    | :? OkObjectResult as (ok: OkObjectResult) ->
        let v: Employee = ok.Value :?> Employee
        Assert.That(v.NameFirst, Is.EqualTo("NewName"))
        Assert.That(v.NameLast, Is.EqualTo("User"))
    | _ -> Assert.Fail("Expected OkObjectResult")

[<Test>]
let ``Update returns NotFound when employee missing`` () =
    // Arrange
    let employeeId = 999L

    let employeesRepoMock = Mock<IEmployeesRepository>()
    employeesRepoMock
        .Setup(fun (m: IEmployeesRepository) -> m.GetByIdAsync(It.IsAny<int64>()))
        .Returns(fun (_: int64) -> async { return Error DatabaseError.NotFound })
    |> ignore

    let employeesReadRepoMock = Mock<IEmployeesReadRepository>()
    let snowMock = Mock<ISnowflakeGenerator>()
    let loggerMock = Mock<ILogger<EmployeesController>>()

    let controller =
        EmployeesController(employeesRepoMock.Object, employeesReadRepoMock.Object, snowMock.Object, loggerMock.Object)

    let patchJson = """[{"op":"replace","path":"/nameFirst","value":"NewName"}]"""
    let patch = JsonSerializer.Deserialize<JsonPatchDocument<Employee>>(patchJson)

    // Act
    let actionResult: ActionResult =
        controller.Update(employeeId, patch).Result

    // Assert
    Assert.That(actionResult, Is.TypeOf<NotFoundResult>())

[<Test>]
let ``Update returns BadRequest on invalid JSON Patch`` () =
    // Arrange
    let employeeId = 1L

    let existingEmp = Employee()
    existingEmp.Id <- employeeId
    existingEmp.NameFirst <- "OldName"
    existingEmp.NameLast <- "User"
    existingEmp.Email <- "old@example.com"
    existingEmp.PhoneNumber <- "+48123456789"

    let employeesRepoMock = Mock<IEmployeesRepository>()
    employeesRepoMock
        .Setup(fun (m: IEmployeesRepository) -> m.GetByIdAsync(It.IsAny<int64>()))
        .Returns(fun (_: int64) -> async { return Ok existingEmp })
    |> ignore

    let employeesReadRepoMock = Mock<IEmployeesReadRepository>()
    let snowMock = Mock<ISnowflakeGenerator>()
    let loggerMock = Mock<ILogger<EmployeesController>>()

    let controller =
        EmployeesController(employeesRepoMock.Object, employeesReadRepoMock.Object, snowMock.Object, loggerMock.Object)

    setupControllerValidation controller

    let patchJson = """[{"op":"remove","path":"/nonexistent"}]"""
    let patch = JsonSerializer.Deserialize<JsonPatchDocument<Employee>>(patchJson)

    employeesRepoMock
        .Setup(fun (m: IEmployeesRepository) -> m.UpdateAsync(It.IsAny<int64>())(It.IsAny<Employee>()))
        .Returns(fun (_key: int64) (_emp: Employee) -> async { return Ok () })
    |> ignore

    // Act
    let actionResult: ActionResult =
        controller.Update(employeeId, patch).Result

    // Assert
    match actionResult with
    | :? ObjectResult as (obj: ObjectResult) ->
        Assert.That(obj.StatusCode, Is.EqualTo(Nullable 400))
    | _ -> Assert.Fail("Expected 400 ObjectResult")
