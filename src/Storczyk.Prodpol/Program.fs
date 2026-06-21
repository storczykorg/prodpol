namespace Storczyk.Prodpol

#nowarn "20"

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



module Program =
    let runServer args =
        let mutable exitCode = 0
        let app = ProdpolServer().Build(args)
        DefaultTypeMap.MatchNamesWithUnderscores = true
        // automatic sql mapping for models
        SqlMappings.applyMappings()

        let result0 = ProgramMigration.applyUpgrade app
        let result1 = result0 && ProgramMigration.applySeeding app

        if not result0 then
            exitCode <- 1
        else if app.Environment.IsDevelopment() && not result1 then
            exitCode <- 2
        else
            app.Run()

        exitCode
    let runOnlyMigrations args =
        let mutable exitCode = 0
        DefaultTypeMap.MatchNamesWithUnderscores = true
        // automatic sql mapping for models
        SqlMappings.applyMappings()

        let app = ProdpolServer().Build(args)

        let result0 = ProgramMigration.applyUpgrade app
        let result1 = result0 && ProgramMigration.applySeeding app

        if not result0 then
            exitCode <- 1
        else if app.Environment.IsDevelopment() && not result1 then
            exitCode <- 2
        exitCode
    let nukeDatabase args =
        let mutable exitCode = 0
        DefaultTypeMap.MatchNamesWithUnderscores = true
        // automatic sql mapping for models
        SqlMappings.applyMappings()

        let app = ProdpolServer().Build(args)

        let result0 = ProgramMigration.nukeDb app

        if not result0 then
            exitCode <- 3
        exitCode
    [<EntryPoint>]
    let main args =
        let mutable result = 0
        // delete schemas (prodpol and prodpol_meta)
        if array.Exists(args, fun (x: string) -> 0 = String.Compare(x, "--nuke", true)) then
            result <- nukeDatabase args

        // exit after migration
        if 0 = result && array.Exists(args, fun (x: string) -> 0 = String.Compare(x, "--migrate", true)) then
            printfn "Running only migrations"
            result <- runOnlyMigrations args
            if result <> 0 then
                eprintf "Migrations failed, code: %s", result
                ()
            else
                printfn "Success"
                ()
        else
            result <- runServer args
        result
