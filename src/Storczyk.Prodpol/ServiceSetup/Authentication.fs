namespace Storczyk.Prodpol.ServiceSetup

open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.BearerToken
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.DependencyInjection

module ServiceAuthentication =
    let configure (services: IServiceCollection) =
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        |> _.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
        |> _.AddCookie("Identity.Application")
        |> _.AddBearerToken(IdentityConstants.BearerScheme: string)
        |> ignore

        services
