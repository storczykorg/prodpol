namespace Storczyk.Prodpol.ServiceSetup

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.DependencyInjection
open Microsoft.IdentityModel.Tokens
open Storczyk.Prodpol.Services

module ServiceAuthentication =
    let configure (services: IServiceCollection) =
        services.AddAuthorization() |> ignore

        services
            .AddAuthentication(fun opts ->
                opts.DefaultScheme <- "Identity.ApplicationOrJwt"
                opts.DefaultChallengeScheme <- "Identity.ApplicationOrJwt")
            .AddPolicyScheme(
                "Identity.ApplicationOrJwt",
                "Bearer or Cookie",
                fun opts ->
                    opts.ForwardDefaultSelector <-
                        fun ctx ->
                            if ctx.Request.Headers.ContainsKey("Authorization") then
                                JwtBearerDefaults.AuthenticationScheme
                            else
                                IdentityConstants.ApplicationScheme
            )
            .AddCookie(IdentityConstants.ApplicationScheme)
            .AddCookie(IdentityConstants.ExternalScheme)
            .AddCookie(IdentityConstants.TwoFactorUserIdScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, ignore)
        |> ignore

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<JwtService>(
                Action<JwtBearerOptions, JwtService>(fun opts jwtService ->
                    opts.TokenValidationParameters <- jwtService.GetValidationParameters()

                    opts.Events <-
                        JwtBearerEvents(
                            OnChallenge =
                                fun ctx ->
                                    ctx.HandleResponse()
                                    ctx.Response.StatusCode <- 401
                                    ctx.Response.ContentType <- "application/json"
                                    ctx.Response.WriteAsync("""{"error":"Unauthorized"}""") |> ignore
                                    Task.CompletedTask
                        ))
            )
        |> ignore

        services
