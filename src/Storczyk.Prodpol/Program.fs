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

    let applyUpgrade (app: WebApplication) : bool =
        use scope = app.Services.CreateScope()
        let upgrader = app.Services.GetRequiredService<PostgresUpgrader>()

        let result = upgrader.Build().PerformUpgrade()

        if not result.Successful then
            app.Logger.LogCritical("Can't apply migration: ", result.Error)
            false
        else
            true
    let applySeeding (app: WebApplication): bool =
        use scope = app.Services.CreateScope()
        let upgrader = app.Services.GetRequiredService<PostgresSeedUpgrader>()

        let result = upgrader.Build().PerformUpgrade()

        if not result.Successful then
            app.Logger.LogCritical("Can't apply migration: ", result.Error)
            false
        else
            true
    
    let mapProperty(_type: Type) (name: string) : PropertyInfo =
        _type.GetProperties().FirstOrDefault(
            fun prop ->
                prop.GetCustomAttributes<ColumnAttribute>(true)
                    .Any(fun attr -> attr.Name = name)
                )
    let addTypeMapping<'T>() =
        SqlMapper.SetTypeMap(
            typeof<'T>,
            CustomPropertyTypeMap(
                typeof<'T>,
                mapProperty
                )
            )
    let ConfigureServices (builder: WebApplicationBuilder) =
        builder.AddPostgresUpgrader()

        let services = builder.Services

        services.AddControllers()
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
        addTypeMapping<EmployeeRead>()
        addTypeMapping<Employee>()
        addTypeMapping<EmployeeRole>()
        
        let builder = WebApplication.CreateBuilder(args)

        ConfigureServices builder

        let app = builder.Build()

        if not (applyUpgrade app) then
            exitCode <- 1
        if app.Environment.IsDevelopment() && not(applySeeding app) then
            exitCode <- 2
        else
            MapApplication app

            app.Run()
        exitCode
    

        
