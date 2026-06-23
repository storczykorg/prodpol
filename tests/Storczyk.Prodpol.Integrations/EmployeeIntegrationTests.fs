module Storczyk.Prodpol.Integrations.EmployeeIntegrationTests

open System
open System.Net.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.Hosting
open Microsoft.Testing.Platform.Services
open NUnit.Framework
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Identity
open Moq
open System.Net
open System.Text.Json
open Storczyk.Database.Services
open Storczyk.Prodpol
open Storczyk.Async
open Storczyk.Prodpol.Core.Utils
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Services

[<AutoOpen>]
module TestHelpers =
    open Moq

    let mockEmployeesReadRepositoryFor (emp: EmployeeRead) : Mock<IEmployeesReadRepository> =
        let mockRepo = Mock<IEmployeesReadRepository>()

        mockRepo
            .Setup(fun (m: IEmployeesReadRepository) -> m.GetByIdAsync(It.IsAny<int64>()))
            .Returns(fun (_: int64) -> async { return emp })
        |> ignore

        mockRepo

    let getClient (app: WebApplication) = app.Services.GetService<HttpClient>()

[<Test>]
let ``GET /api/data/employees/26 returns mocked employee`` () =
    let emp = EmployeeRead()
    emp.Id <- 26L
    emp.NormalizedEmail <- "system@storczyk.org"
    emp.FullName <- "System Informatyczny"
    emp.NormalizedName <- "system informatyczny"
    emp.RoleName <- Some "special.system"
    emp.RoleId <- Some 1
    emp.Email <- "system@storczyk.org"
    emp.NameFirst <- "System"
    emp.NameLast <- "Informatyczny"
    emp.PhoneNumber <- "+48111111111"
    emp.PasswordHash <- None
    emp.CreatedAt <- DateTime(2026, 5, 11, 16, 28, 03, 522)
    emp.Enabled <- true

    let mockRepo = mockEmployeesReadRepositoryFor emp

    use server =
        ProdpolServer()
            .BuildTest(
                [||],
                fun s ->
                    s.AddSingleton<IEmployeesReadRepository>(mockRepo.Object) |> ignore
                    ()
            )

    use client = getClient server

    let resp = client.GetAsync("http://localhost:5000/api/data/employees/26").Result
    Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK))

    let body = resp.Content.ReadAsStringAsync().Result
    let doc = JsonDocument.Parse(body).RootElement

    Assert.That(doc.GetProperty("normalizedEmail").GetString(), Is.EqualTo("system@storczyk.org"))
    Assert.That(doc.GetProperty("fullName").GetString(), Is.EqualTo("System Informatyczny"))
    Assert.That(doc.GetProperty("normalizedName").GetString(), Is.EqualTo("system informatyczny"))
    Assert.That(doc.GetProperty("roleName").GetString(), Is.EqualTo("special.system"))
    Assert.That(doc.GetProperty("id").GetInt32(), Is.EqualTo(26))
    Assert.That(doc.GetProperty("roleId").GetInt32(), Is.EqualTo(1))
    Assert.That(doc.GetProperty("email").GetString(), Is.EqualTo("system@storczyk.org"))
    Assert.That(doc.GetProperty("nameFirst").GetString(), Is.EqualTo("System"))
    Assert.That(doc.GetProperty("nameLast").GetString(), Is.EqualTo("Informatyczny"))
    Assert.That(doc.GetProperty("phoneNumber").GetString(), Is.EqualTo("+48111111111"))
    Assert.That(doc.GetProperty("passwordHash").ValueKind, Is.EqualTo(JsonValueKind.Null))
    Assert.That(doc.GetProperty("createdAt").GetString().StartsWith("2026-05-11T16:28:03.522"), Is.True)
    Assert.That(doc.GetProperty("enabled").GetBoolean(), Is.EqualTo(true))

[<Test>]
let ``POST /api/data/employees/ creates employee and returns 200`` () =
    let snowflakeId = 999L

    let snowMock = Mock<ISnowflakeGenerator>()

    snowMock.Setup(fun m -> m.GetSnowflake(It.IsAny<DateTime>())).Returns(snowflakeId)
    |> ignore

    let employeesRepo = Mock<IEmployeesRepository>()

    employeesRepo
        .Setup(fun (m: IEmployeesRepository) -> m.AddAsync(It.IsAny<Employee>()))
        .Returns(fun (_: Employee) -> async { return () })
    |> ignore

    let createdEmp = EmployeeRead()
    createdEmp.Id <- snowflakeId
    createdEmp.Email <- "new@example.com"
    createdEmp.NameFirst <- "New"
    createdEmp.NameLast <- "Employee"
    createdEmp.PhoneNumber <- "+48111111111"
    createdEmp.NormalizedEmail <- "NEW@EXAMPLE.COM"
    createdEmp.FullName <- "New Employee"
    createdEmp.NormalizedName <- "NEW EMPLOYEE"
    createdEmp.Enabled <- true
    createdEmp.CreatedAt <- DateTime.UtcNow

    let employeesReadRepo = Mock<IEmployeesReadRepository>()

    employeesReadRepo
        .Setup(fun (m: IEmployeesReadRepository) -> m.GetByIdAsync(It.IsAny<int64>()))
        .Returns(fun (_: int64) -> async { return createdEmp })
    |> ignore

    let passwordHasher = Mock<IPasswordHasher<Employee>>()

    passwordHasher.Setup(fun m -> m.HashPassword(It.IsAny<Employee>(), It.IsAny<string>())).Returns("hashed-pwd")
    |> ignore

    use server =
        ProdpolServer()
            .BuildTest(
                [||],
                fun s ->
                    s.AddSingleton<IEmployeesRepository>(employeesRepo.Object) |> ignore
                    s.AddSingleton<IEmployeesReadRepository>(employeesReadRepo.Object) |> ignore
                    s.AddSingleton<ISnowflakeGenerator>(snowMock.Object) |> ignore
                    s.AddSingleton<IPasswordHasher<Employee>>(passwordHasher.Object) |> ignore
                    ()
            )

    use client = getClient server

    let body =
        JsonSerializer.Serialize(
            {| email = "new@example.com"
               nameFirst = "New"
               nameLast = "Employee"
               phoneNumber = "+48111111111" |}
        )

    let content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")

    let resp =
        client.PostAsync("http://localhost:5000/api/data/employees/", content).Result

    Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK))

    let respBody = resp.Content.ReadAsStringAsync().Result
    let doc = JsonDocument.Parse(respBody).RootElement
    Assert.That(doc.GetProperty("id").GetInt32(), Is.EqualTo(int snowflakeId))
    Assert.That(doc.GetProperty("email").GetString(), Is.EqualTo("new@example.com"))
    Assert.That(doc.GetProperty("nameFirst").GetString(), Is.EqualTo("New"))

[<Test>]
let ``PATCH /api/data/employees/1 updates employee name`` () =
    let employeeId = 1L

    let existingEmp = Employee()
    existingEmp.Id <- employeeId
    existingEmp.NameFirst <- "OldName"
    existingEmp.NameLast <- "User"
    existingEmp.Email <- "old@example.com"
    existingEmp.PhoneNumber <- "+48123456789"

    let employeesRepo = Mock<IEmployeesRepository>()

    employeesRepo
        .Setup(fun (m: IEmployeesRepository) -> m.GetByIdAsync(It.IsAny<int64>()))
        .Returns(fun (_: int64) -> async { return existingEmp })
    |> ignore

    employeesRepo
        .Setup(fun (m: IEmployeesRepository) -> m.UpdateAsync(It.IsAny<int64>(), It.IsAny<Employee>()))
        .Returns(fun (_key: int64, _emp: Employee) -> async { return () })
    |> ignore

    let employeesReadRepo = Mock<IEmployeesReadRepository>()
    let snowMock = Mock<ISnowflakeGenerator>()

    use server =
        ProdpolServer()
            .BuildTest(
                [||],
                fun s ->
                    s.AddSingleton<IEmployeesRepository>(employeesRepo.Object) |> ignore
                    s.AddSingleton<IEmployeesReadRepository>(employeesReadRepo.Object) |> ignore
                    s.AddSingleton<ISnowflakeGenerator>(snowMock.Object) |> ignore
                    ()
            )

    use client = getClient server

    let patchBody = """[{"op":"replace","path":"/nameFirst","value":"UpdatedName"}]"""
    let content = new StringContent(patchBody, System.Text.Encoding.UTF8, "text/json")

    let resp =
        client.PatchAsync("http://localhost:5000/api/data/employees/1", content).Result

    Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK))

    let respBody = resp.Content.ReadAsStringAsync().Result
    let doc = JsonDocument.Parse(respBody).RootElement
    Assert.That(doc.GetProperty("nameFirst").GetString(), Is.EqualTo("UpdatedName"))
    Assert.That(doc.GetProperty("nameLast").GetString(), Is.EqualTo("User"))

[<Test>]
let ``PATCH /api/data/employees/999 returns 404`` () =
    let employeesRepo = Mock<IEmployeesRepository>()

    employeesRepo
        .Setup(fun (m: IEmployeesRepository) -> m.GetByIdAsync(It.IsAny<int64>()))
        .Returns(fun (_: int64) -> async { return raise (NotFoundException "Resource not found.") })
    |> ignore

    let employeesReadRepo = Mock<IEmployeesReadRepository>()
    let snowMock = Mock<ISnowflakeGenerator>()

    use server =
        ProdpolServer()
            .BuildTest(
                [||],
                fun s ->
                    s.AddSingleton<IEmployeesRepository>(employeesRepo.Object) |> ignore
                    s.AddSingleton<IEmployeesReadRepository>(employeesReadRepo.Object) |> ignore
                    s.AddSingleton<ISnowflakeGenerator>(snowMock.Object) |> ignore
                    ()
            )

    use client = getClient server

    let patchBody = """[{"op":"replace","path":"/nameFirst","value":"UpdatedName"}]"""
    let content = new StringContent(patchBody, System.Text.Encoding.UTF8, "text/json")

    let resp =
        client.PatchAsync("http://localhost:5000/api/data/employees/999", content).Result

    Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound))
