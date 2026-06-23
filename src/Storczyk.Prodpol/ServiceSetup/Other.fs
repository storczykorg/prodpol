namespace Storczyk.Prodpol.ServiceSetup

open Microsoft.Extensions.DependencyInjection
open Storczyk.Database.Services
open Storczyk.Prodpol.Core.Services
open Storczyk.Prodpol.Core.Utils.RegisterServiceExtensions

module ServiceOther =
    let configure (services: IServiceCollection) =
        services
        |> _.AddOpenApi("v0")
        |> _.AddPostgresUpgrader()
        |> _.AddSingleton(Snowflake.DefaultSnowflakeOptions)
        |> _.RegisterFromRuntime()
        |> ignore

        services
