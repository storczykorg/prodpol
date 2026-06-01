namespace Storczyk.Prodpol

#nowarn "20"

open System
open System.Linq
open System.Reflection
open Dapper
open LinqToDB.Mapping
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Storczyk.Database.Services
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Services

module Program =
    let mutable exitCode = 0
    let ConfigureServices (builder: WebApplicationBuilder) =
        builder.AddPostgresUpgrader()

        let services = builder.Services

        builder.Services.AddControllers(fun options ->
            options.ModelBinderProviders.Insert(0, Utils.FSharpOptionModelBinderProvider())
        )


        services.AddOpenApi("v0")

        builder.AddNpgsqlDataSource("postgresdb")

        services.AddPostgresRepositories()

        services.AddSingleton<ISnowflakeGenerator>(SnowflakeGenerator Snowflake.DefaultSnowflakeOptions)

    let MapApplication (app: WebApplication) =
        if app.Environment.IsDevelopment() then
            app.MapOpenApi("/openapi/{documentName}.yaml") |> ignore

        app.UseAuthorization()
        app.MapControllers()

    [<EntryPoint>]
    let main args =
        DefaultTypeMap.MatchNamesWithUnderscores = true
        ProgramMigration.addTypeMapping<EmployeeRead>()
        ProgramMigration.addTypeMapping<Employee>()
        ProgramMigration.addTypeMapping<EmployeeRole>()
        
        let builder = WebApplication.CreateBuilder(args)

        ConfigureServices builder

        let app = builder.Build()

        if not (ProgramMigration.applyUpgrade app) then
            exitCode <- 1
        if app.Environment.IsDevelopment() && not(ProgramMigration.applySeeding app) then
            exitCode <- 2
        else
            MapApplication app

            app.Run()
        exitCode
    

        
