namespace Storczyk.Prodpol

#nowarn "20"

open LinqToDB.Internal.DataProvider.PostgreSQL
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Npgsql
open Storczyk.Database.Services
open System.Linq
open System.Reflection
open Dapper
open LinqToDB.Mapping
open System

module ProgramMigration =
    let applyUpgrade (app: WebApplication) : bool =
        use scope = app.Services.CreateScope()
        let upgrader = scope.ServiceProvider.GetRequiredService<PostgresUpgrader>()

        let result = upgrader.Build().PerformUpgrade()

        if not result.Successful then
            app.Logger.LogCritical("Can't apply migration: ", result.Error)
            false
        else
            true

    let applySeeding (app: WebApplication) : bool =
        use scope = app.Services.CreateScope()
        let upgrader = scope.ServiceProvider.GetRequiredService<PostgresSeedUpgrader>()

        let result = upgrader.Build().PerformUpgrade()

        if not result.Successful then
            app.Logger.LogCritical("Can't apply migration: ", result.Error)
            false
        else
            true

    let nukeDb (app: WebApplication) : bool =
        use scope = app.Services.CreateScope()
        let upgrader = scope.ServiceProvider.GetRequiredService<PostgresNuke>()

        let result = upgrader.Build().PerformUpgrade()

        if not result.Successful then
            app.Logger.LogCritical("Can't nuke database: ", result.Error)
            false
        else
            true
