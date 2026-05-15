namespace Storczyk.Prodpol
#nowarn "20"
open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Npgsql
open Storczyk.Database.Services

module Program =
    let exitCode = 0

    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)
        
        builder.AddPostgresUpgrader()

        let services = builder.Services
            
        services.AddControllers()
        
        builder.AddNpgsqlDataSource("postgresdb")

        let app = builder.Build()
        
        use scope = app.Services.CreateScope()
        let upgrader = app.Services.GetRequiredService<PostgresUpgrader>()
        
        let result = upgrader.Build().PerformUpgrade()
        
        if not result.Successful then
            app.Logger.LogCritical("Can't apply migration: ", result.Error)
            1
        else
            app.UseAuthorization()
            app.MapControllers()

            app.Run()

            exitCode
