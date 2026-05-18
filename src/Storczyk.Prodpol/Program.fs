namespace Storczyk.Prodpol

#nowarn "20"

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Storczyk.Database.Services
open Storczyk.Prodpol.Core.Services

module Program =
    let exitCode = 0

    let applyUpgrade (app: WebApplication) : bool =
        use scope = app.Services.CreateScope()
        let upgrader = app.Services.GetRequiredService<PostgresUpgrader>()

        let result = upgrader.Build().PerformUpgrade()

        if not result.Successful then
            app.Logger.LogCritical("Can't apply migration: ", result.Error)
            false
        else
            true

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

        let builder = WebApplication.CreateBuilder(args)

        ConfigureServices builder

        let app = builder.Build()

        if not (applyUpgrade app) then
            1
        else
            MapApplication app

            app.Run()
            exitCode
