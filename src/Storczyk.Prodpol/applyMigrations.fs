namespace Storczyk.Prodpol

#nowarn "20"

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
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
    let applySeeding (app: WebApplication): bool =
        use scope = app.Services.CreateScope()
        let upgrader = scope.ServiceProvider.GetRequiredService<PostgresSeedUpgrader>()

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