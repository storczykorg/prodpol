namespace Storczyk.Prodpol

#nowarn "20"

open System
open System.Text.Json.Serialization
open System.Threading.Tasks
open Dapper
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Npgsql
open Storczyk.Database.Services
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Services
open Storczyk.Prodpol.Utils


type ProdpolServer() =
    member this.Configure(builder: IHostApplicationBuilder) =

        builder.Configuration.AddUserSecrets()

        this.ConfigureServices(builder.Services)
        //.NET Aspire specific method
        let enumMappings (dataSource: NpgsqlDataSourceBuilder) =
            dataSource.MapEnum<EmployeeOrderKeys>("prodpol.employee_ordering_keys")
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

        services.AddOpenTelemetry()
            .WithLogging()
            .WithMetrics()
            .WithTracing()
            |> ignore

        services.AddOpenApi("v0")

        services.AddPostgresRepositories()
        services.AddPostgresUpgrader()

        services.AddSingleton<ISnowflakeGenerator>(SnowflakeGenerator Snowflake.DefaultSnowflakeOptions)

        ()

    member this.MapApplication(app: WebApplication) =
        app.UseRouting()

        if app.Environment.IsDevelopment() then
            app.MapOpenApi("/openapi/{documentName}.yaml") |> ignore

        app.UseAuthorization()
        app.MapControllers()
        app

    member this.Build(args: string array) =
        let builder = WebApplication.CreateBuilder(args)

        this.Configure builder

        builder.Build() |> this.MapApplication

    member this.BuildTest(args: string array, ?testServices: IServiceCollection -> unit) =
        let builder = WebApplication.CreateBuilder(args)

        builder.Services.AddHttpClient(fun x ->
            let add = "http://localhost:5000/api/"
            x.BaseAddress <- Uri(add))

        this.Configure builder

        (Option.defaultValue ignore testServices) builder.Services
        let app = builder.Build() |> this.MapApplication

        app.Start()
        app

    member this.WebApplicationBuilder(args: string array) =
        let builder = WebApplication.CreateBuilder(args)

        this.Configure builder |> ignore

        builder

module Program =
    [<EntryPoint>]
    let main args =
        let mutable exitCode = 0
        DefaultTypeMap.MatchNamesWithUnderscores = true
        ProgramMigration.addTypeMapping<EmployeeRead> ()
        ProgramMigration.addTypeMapping<Employee> ()
        ProgramMigration.addTypeMapping<EmployeeRole> ()

        let app = ProdpolServer().Build(args)

        let result0 = ProgramMigration.applyUpgrade app
        let result1 = ProgramMigration.applySeeding app

        if not result0 then
            exitCode <- 1

        if app.Environment.IsDevelopment() && not result1 then
            exitCode <- 2
        else
            app.Run()

        exitCode
