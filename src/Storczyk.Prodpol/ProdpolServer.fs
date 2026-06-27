namespace Storczyk.Prodpol

open System
open System.Text
open System.Threading.Tasks
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Routing
open Microsoft.AspNetCore.WebUtilities
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open System.Security.Claims
open Npgsql
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Data.Services.Identity
open Storczyk.Prodpol.ServiceSetup
open Storczyk.Prodpol.Services

type AdminPasswordResetTokenRequest = { Email: string }

[<AutoOpen>]
module ServerHelper =
    let inline withObj (receiver: 'T) ([<InlineIfLambda>] block: 'T -> 'U) : 'U = block receiver

type ProdpolServer() =
    member this.Configure(builder: IHostApplicationBuilder) : IHostApplicationBuilder =
        builder.Configuration.AddUserSecrets() |> ignore

        this.ConfigureServices(builder.Services)

        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"))
        |> ignore

        builder.Services.AddSingleton<JwtService>() |> ignore

        let enumMappings (dataSource: NpgsqlDataSourceBuilder) =
            dataSource.MapEnum<EmployeeOrderKeys>("prodpol.employee_ordering_keys")
            |> ignore

            dataSource.MapEnum<CustomerOrderKeys>("prodpol.customer_ordering_keys")
            |> ignore

            dataSource.MapEnum<ProductOrderKeys>("prodpol.product_ordering_keys") |> ignore

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

        withObj (app.MapGroup("/api/employee/auth")) (fun group ->
            group.MapIdentityApi<Employee>() |> ignore

            group.MapPost(
                "logout",
                Func<HttpContext, Task<IResult>>(fun (ctx) ->
                    task {
                        do! ctx.SignOutAsync()
                        return Results.Ok()
                    })
            )
            |> ignore

            group.WithTags("Authentication") |> ignore

            group.MapGet(
                "me",
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

            group.MapPost(
                "jwt-login",
                Func<JwtLoginRequest, HttpContext, Task<IResult>>(fun req ctx ->
                    task {
                        let userManager = ctx.RequestServices.GetRequiredService<UserManager<Employee>>()

                        let signInManager = ctx.RequestServices.GetRequiredService<EmployeeSignInManager>()

                        let jwtService = ctx.RequestServices.GetRequiredService<JwtService>()

                        let! user = userManager.FindByEmailAsync(req.Email)

                        if isNull (box user) then
                            return Results.Unauthorized()
                        else
                            let! valid = userManager.CheckPasswordAsync(user, req.Password)

                            if not valid then
                                return Results.Unauthorized()
                            else
                                let claims =
                                    [ Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                                      Claim(ClaimTypes.Email, user.Email)
                                      Claim(ClaimTypes.GivenName, user.NameFirst)
                                      Claim(ClaimTypes.Surname, user.NameLast)
                                      Claim(ClaimTypes.Role, "employee")
                                      Claim("enabled", user.Enabled.ToString()) ]

                                let accessToken = jwtService.GenerateAccessToken(claims)
                                let refreshToken = jwtService.GenerateRefreshToken(user.Id)

                                return
                                    Results.Ok(
                                        { AccessToken = accessToken
                                          RefreshToken = refreshToken
                                          ExpiresIn = 900.0 }
                                    )
                    })
            )
            |> ignore

            group.MapPost(
                "jwt-refresh",
                Func<JwtRefreshRequest, HttpContext, Task<IResult>>(fun req ctx ->
                    task {
                        let userManager = ctx.RequestServices.GetRequiredService<UserManager<Employee>>()

                        let jwtService = ctx.RequestServices.GetRequiredService<JwtService>()

                        match jwtService.TryValidateRefreshToken(req.RefreshToken) with
                        | None -> return Results.Unauthorized()
                        | Some userId ->
                            let! user = userManager.FindByIdAsync(userId.ToString())

                            if isNull (box user) then
                                return Results.Unauthorized()
                            else
                                let claims =
                                    [ Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                                      Claim(ClaimTypes.Email, user.Email)
                                      Claim(ClaimTypes.GivenName, user.NameFirst)
                                      Claim(ClaimTypes.Surname, user.NameLast)
                                      Claim(ClaimTypes.Role, "employee")
                                      Claim("enabled", user.Enabled.ToString()) ]

                                let accessToken = jwtService.GenerateAccessToken(claims)
                                let refreshToken = jwtService.GenerateRefreshToken(user.Id)

                                return
                                    Results.Ok(
                                        { AccessToken = accessToken
                                          RefreshToken = refreshToken
                                          ExpiresIn = 900.0 }
                                    )
                    })
            )
            |> ignore

            ())



        app.UseAuthentication() |> ignore
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
