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
open Moq
open System.Net
open System.Text.Json
open Storczyk.Database.Services
open Storczyk.Prodpol
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models

[<AutoOpen>]
module TestHelpers =
    open Moq

    let mockEmployeesReadRepositoryFor (emp: EmployeeRead) : Mock<IEmployeesReadRepository> =
        let mockRepo = Mock<IEmployeesReadRepository>()

        mockRepo
            .Setup(fun (m: IEmployeesReadRepository) -> m.GetByIdAsync(It.IsAny<int64>()))
            .Returns(fun (_: int64) -> async { return Ok(emp) })
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
                    s.AddNullRepositories().AddSingleton<IEmployeesReadRepository>(mockRepo.Object)
                    |> ignore
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
