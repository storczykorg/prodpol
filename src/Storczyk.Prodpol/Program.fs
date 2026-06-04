namespace Storczyk.Prodpol

#nowarn "20"

open System
open System.Linq
open System.Reflection
open Dapper
open LinqToDB.Mapping
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Npgsql
open Storczyk.Database.Services
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Services


type ProdpolServer() =
    member this.Configure(builder: IHostApplicationBuilder) =

        this.ConfigureServices(builder.Services)
        //.NET Aspire specific method
        let enumMappings (dataSource: NpgsqlDataSourceBuilder) =
            dataSource.MapEnum<EmployeeOrderKeys>("prodpol.employee_ordering_keys")
            ()
        
        builder.AddNpgsqlDataSource(connectionName = "postgresdb",
                                    configureDataSourceBuilder = enumMappings)
        builder

    member this.ConfigureServices(services: IServiceCollection) =
        let mainAs = typeof<ProdpolServer>.Assembly

        services
            .AddControllers(fun options ->
                options.ModelBinderProviders.Insert(0, Utils.FSharpOptionModelBinderProvider()))
            .AddApplicationPart(mainAs)

        services.AddPostgresRepositories()
        services.AddPostgresUpgrader()

        services.AddSingleton<ISnowflakeGenerator>(SnowflakeGenerator Snowflake.DefaultSnowflakeOptions)

        services.AddOpenApi("v0")

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

        if not (ProgramMigration.applyUpgrade app) then
            exitCode <- 1

        if app.Environment.IsDevelopment() && not (ProgramMigration.applySeeding app) then
            exitCode <- 2
        else
            app.Run()

        exitCode
