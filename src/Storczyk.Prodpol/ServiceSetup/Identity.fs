namespace Storczyk.Prodpol.ServiceSetup

open System
open Microsoft.AspNetCore.DataProtection
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.DependencyInjection
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Services
open Storczyk.Prodpol.Data.Services
open Storczyk.Prodpol.Data.Services.Identity

module ServiceIdentity =
    let configure (services: IServiceCollection) =
        services.AddDataProtection() |> ignore
        services.AddSingleton<TimeProvider, ProdpolTimeProvider>() |> ignore
        services.AddTransient<UserManager<Employee>>() |> ignore
        services.AddTransient<EmployeeSignInManager>() |> ignore
        services.AddTransient<IEmailSender<Employee>, LoggingEmailSender>() |> ignore

        services.AddScoped<ILookupNormalizer, LowerInvariantLookupNormalizer>()
        |> ignore

        services
        |> _.AddIdentityCore<Employee>()
        |> _.AddDefaultTokenProviders()
        |> _.AddPasswordValidator<PasswordValidator<Employee>>()
        |> _.AddUserValidator<EmployeeIdValidator>()
        |> _.AddUserStore<PgEmployeeUserStore>()
        |> _.AddUserManager<EmployeeUserManager>()
        |> _.AddSignInManager<EmployeeSignInManager>()
        |> _.AddClaimsPrincipalFactory<EmployeeClaimsPrincipalFactory>()
        |> _.AddApiEndpoints()
        |> ignore

        services
