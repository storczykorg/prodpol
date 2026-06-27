namespace Storczyk.Prodpol.Data.Services.Identity

open System.Security.Claims
open System.Threading.Tasks
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.Options
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models

type EmployeeClaimsPrincipalFactory
    (userManager: UserManager<Employee>, options: IOptions<IdentityOptions>, employeesRead: IEmployeesReadRepository) =
    inherit UserClaimsPrincipalFactory<Employee>(userManager, options)

    member private this.BaseClaimsAsync(user: Employee) : Task<ClaimsIdentity> = base.GenerateClaimsAsync(user)

    override this.GenerateClaimsAsync(user: Employee) =
        task {
            let! identity = this.BaseClaimsAsync(user)

            identity.AddClaim(Claim(ClaimTypes.GivenName, user.NameFirst))
            identity.AddClaim(Claim(ClaimTypes.Surname, user.NameLast))
            identity.AddClaim(Claim(ClaimTypes.Email, user.Email))
            identity.AddClaim(Claim("enabled", user.Enabled.ToString()))
            identity.AddClaim(Claim(ClaimTypes.Role, "employee"))
            identity.AddClaim(Claim(ClaimTypes.NameIdentifier, user.Id.ToString()))

            user.RoleId
            |> Option.iter (fun rid -> identity.AddClaim(Claim("employee_role_id", rid.ToString())))

            let! empRead = employeesRead.GetByIdAsync(user.Id)

            empRead.RoleName
            |> Option.iter (fun rn -> identity.AddClaim(Claim(ClaimTypes.Role, rn)))

            return identity
        }
