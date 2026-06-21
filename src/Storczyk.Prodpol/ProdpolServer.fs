namespace Storczyk.Prodpol

open System
open System.Reflection
open System.Text.Json.Serialization
open System.Threading.Tasks
open Dapper
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Npgsql
open OpenTelemetry
open OpenTelemetry.Logs
open OpenTelemetry.Metrics
open OpenTelemetry.Trace
open Storczyk.Database.Services
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Services
open Storczyk.Prodpol.Core.Utils
open Storczyk.Prodpol.Core.Utils.RegisterServiceExtensions
open Storczyk.Prodpol.Utils

type ProdpolServer() =
    member this.Configure(builder: IHostApplicationBuilder) =

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

        services
            .AddIdentityCore<Employee>()
            .AddPasswordValidator<PasswordValidator<Employee>>()
            .AddUserStore<PgEmployeeUserStore>()
        |> ignore

        services
            .AddOpenApi("v0")
            .AddPostgresUpgrader()
            .AddSingleton(Snowflake.DefaultSnowflakeOptions)
            .RegisterFromRuntime()
        |> ignore

        ()

    member this.MapApplication(app: WebApplication) =
        app.UseRouting() |> ignore

        if app.Environment.IsDevelopment() then
            app.MapOpenApi("/openapi/{documentName}.yaml") |> ignore

        app.UseAuthorization() |> ignore
        app.MapControllers() |> ignore

        app.UseOpenTelemetryPrometheusScrapingEndpoint() |> ignore
        app

    member this.Build(args: string array) =
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

    member this.WebApplicationBuilder(args: string array) =
        let builder = WebApplication.CreateBuilder(args)

        this.Configure builder |> ignore

        builder
