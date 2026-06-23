namespace Storczyk.Prodpol

open System
open System.Text
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Routing
open Microsoft.AspNetCore.WebUtilities
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Npgsql
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.ServiceSetup

type AdminPasswordResetTokenRequest = { Email: string }

type ProdpolServer() =
    member this.Configure(builder: IHostApplicationBuilder) : IHostApplicationBuilder =
        builder.Configuration.AddUserSecrets() |> ignore

        this.ConfigureServices(builder.Services)

        let enumMappings (dataSource: NpgsqlDataSourceBuilder) =
            dataSource.MapEnum<EmployeeOrderKeys>("prodpol.employee_ordering_keys")
            |> ignore

            dataSource.ConnectionStringBuilder.MaxAutoPrepare <- 100
            dataSource.ConnectionStringBuilder.MinPoolSize <- 8
            ()

        builder.AddNpgsqlDataSource(connectionName = "postgresdb", configureDataSourceBuilder = enumMappings)
        builder

    member this.ConfigureServices(services: IServiceCollection) =
        services
        |> ServiceControllers.configure
        |> ServiceTelemetry.configure
        |> ServiceAuthentication.configure
        |> ServiceIdentity.configure
        |> ServiceLinqToDb.configure
        |> ServiceOther.configure
        |> ignore

    member this.MapApplication(app: WebApplication) : WebApplication =
        app.UseRouting() |> ignore

        if app.Environment.IsDevelopment() then
            app.MapOpenApi("/openapi/{documentName}.yaml") |> ignore

        app.MapGroup("/api/employee/auth").MapIdentityApi<Employee>() |> ignore

        app.MapPost(
            "/api/employee/auth/admin/reset-password-token",
            Func<AdminPasswordResetTokenRequest, HttpContext, Task<IResult>>(fun req context ->
                task {
                    let userManager =
                        context.RequestServices.GetRequiredService<UserManager<Employee>>()

                    let logger = context.RequestServices.GetRequiredService<ILogger<ProdpolServer>>()
                    let! user = userManager.FindByEmailAsync(req.Email)

                    match box user with
                    | null -> return Results.Ok()
                    | _ ->
                        let! token = userManager.GeneratePasswordResetTokenAsync(user)
                        let encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token))
                        logger.LogInformation("Admin password reset token for {Email}: {Token}", req.Email, encoded)
                        return Results.Ok()
                })
        )
        |> ignore

        app.MapGet(
            "/api/employee/auth/me",
            Func<HttpContext, Task<IResult>>(fun context ->
                task {
                    let userManager =
                        context.RequestServices.GetRequiredService<UserManager<Employee>>()

                    let employeesRead =
                        context.RequestServices.GetRequiredService<IEmployeesReadRepository>()

                    let userIdStr = userManager.GetUserId(context.User)

                    if String.IsNullOrEmpty(userIdStr) then
                        return Results.Unauthorized()
                    else
                        let userId = Int64.Parse(userIdStr)
                        let! employee = employeesRead.GetByIdAsync(userId)
                        return Results.Ok(employee)
                })
        )
        |> ignore

        app.UseAuthorization() |> ignore
        app.MapControllers() |> ignore

        app.UseOpenTelemetryPrometheusScrapingEndpoint() |> ignore
        app

    member this.Build(args: string array) : WebApplication =
        let builder = WebApplication.CreateBuilder(args)

        this.Configure builder |> ignore

        builder.Build() |> this.MapApplication

    member this.BuildTest(args: string array, ?testServices: IServiceCollection -> unit) =
        let builder = WebApplication.CreateBuilder(args)

        builder.Services.AddHttpClient(fun x ->
            let add = "http://localhost:5000/api/"
            x.BaseAddress <- Uri(add))
        |> ignore

        this.Configure builder |> ignore

        (Option.defaultValue ignore testServices) builder.Services
        let app = builder.Build() |> this.MapApplication

        app.Start()
        app

    member this.WebApplicationBuilder(args: string array) : WebApplicationBuilder =
        let builder = WebApplication.CreateBuilder(args)

        this.Configure builder |> ignore

        builder
