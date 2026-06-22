namespace Storczyk.Prodpol.Data.Services.Identity

open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open Storczyk.Prodpol.Core.Models

type EmployeeSignInManager
    (
        userManager: UserManager<Employee>,
        contextAccessor: IHttpContextAccessor,
        claimsFactory: IUserClaimsPrincipalFactory<Employee>,
        optionsAccessor: IOptions<IdentityOptions>,
        logger: ILogger<SignInManager<Employee>>,
        schemes: IAuthenticationSchemeProvider,
        confirmation: IUserConfirmation<Employee>
    ) =
    inherit SignInManager<Employee>(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)