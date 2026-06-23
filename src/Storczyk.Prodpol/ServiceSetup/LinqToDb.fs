namespace Storczyk.Prodpol.ServiceSetup

open System
open LinqToDB
open LinqToDB.AspNet
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Storczyk.Prodpol.Data.Services.Identity

module ServiceLinqToDb =
    let configure (services: IServiceCollection) =
        services.AddLinqToDBContext<IIdentityDatabase, IdentityDatabase>(
            Func<IServiceProvider, DataOptions, DataOptions>(fun provider options ->
                let config = provider.GetRequiredService<IConfiguration>()
                let connString = config.GetConnectionString("postgresdb")
                options.UsePostgreSQL(connectionString = connString))
        )
        |> ignore

        services
