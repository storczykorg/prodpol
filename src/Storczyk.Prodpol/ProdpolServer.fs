namespace Storczyk.Prodpol

open System
open System.Net
open System.Text.Json.Serialization
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.DataProtection
open Microsoft.AspNetCore.DataProtection.KeyManagement.Internal
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Routing
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Npgsql
open OpenTelemetry
open OpenTelemetry.Metrics
open OpenTelemetry.Trace
open Storczyk.Database.Services
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Services
open Storczyk.Prodpol.Core.Utils.RegisterServiceExtensions
open Storczyk.Prodpol.Data.Services
open Storczyk.Prodpol.Data.Services.Identity
open Storczyk.Prodpol.Utils

type ProdpolServer() =
    member this.Configure(builder: IHostApplicationBuilder): IHostApplicationBuilder =

        builder.Configuration.AddUserSecrets() |> ignore

        this.ConfigureServices(builder.Services)
        //.NET Aspire specific method
        let enumMappings (dataSource: NpgsqlDataSourceBuilder) =
            dataSource.MapEnum<EmployeeOrderKeys>("prodpol.employee_ordering_keys")
            |> ignore

            dataSource.ConnectionStringBuilder.MaxAutoPrepare <- 100
            dataSource.ConnectionStringBuilder.MinPoolSize <- 8
            ()

        builder.AddNpgsqlDataSource(connectionName = "postgresdb", configureDataSourceBuilder = enumMappings)
        builder

    member this.ConfigureServices(services: IServiceCollection) =
        let mainAs = typeof<ProdpolServer>.Assembly

        services
            .AddControllers()
            .AddMvcOptions(fun options ->
                options.ModelBinderProviders.Insert(0, FSharpOptionModelBinderProvider())
                ())
            .AddJsonOptions(fun options ->
                // Dodaj wsparcie dla typów F# (Option, Discriminated Unions, Records)
                let fsharpConverter = JsonFSharpConverter(JsonFSharpOptions.FSharpLuLike())
                options.JsonSerializerOptions.Converters.Insert(0, fsharpConverter))
            .AddApplicationPart(mainAs)
        |> ignore

        services
            .AddOpenTelemetry()
            .WithLogging(fun logging -> logging |> ignore)
            .WithMetrics(fun metrics ->
                metrics.AddPrometheusExporter().AddAspNetCoreInstrumentation().AddNpgsqlInstrumentation()
                |> ignore)
            .WithTracing(fun tracing -> tracing.AddAspNetCoreInstrumentation().AddNpgsql() |> ignore)
            .UseOtlpExporter()
        |> ignore

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddBearerToken()
            .AddCookie() |> ignore

        services.AddDataProtection() |> ignore
        services.AddSingleton<TimeProvider, ProdpolTimeProvider>() |> ignore
        services.AddTransient<EmployeeSignInManager>() |> ignore

        services
            .AddIdentityCore<Employee>()
            .AddDefaultTokenProviders()
            .AddPasswordValidator<PasswordValidator<Employee>>()
            .AddUserStore<PgEmployeeUserStore>()
            .AddUserManager<UserManager<Employee>>()
            .AddSignInManager<EmployeeSignInManager>()
            .AddApiEndpoints()
        |> ignore

        services
            .AddOpenApi("v0")
            .AddPostgresUpgrader()
            .AddSingleton(Snowflake.DefaultSnowflakeOptions)
            .RegisterFromRuntime()
        |> ignore

        ()

    member this.MapApplication(app: WebApplication): WebApplication =
        app.UseRouting() |> ignore

        if app.Environment.IsDevelopment() then
            app.MapOpenApi("/openapi/{documentName}.yaml") |> ignore

        app.MapGroup("/api").MapIdentityApi() |> ignore

        app.UseAuthorization() |> ignore
        app.MapControllers() |> ignore

        app.UseOpenTelemetryPrometheusScrapingEndpoint() |> ignore
        app

    member this.Build(args: string array): WebApplication =
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

    member this.WebApplicationBuilder(args: string array): WebApplicationBuilder =
        let builder = WebApplication.CreateBuilder(args)

        this.Configure builder |> ignore

        builder
