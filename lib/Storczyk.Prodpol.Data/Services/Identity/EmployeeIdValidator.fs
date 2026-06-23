namespace Storczyk.Prodpol.Data.Services.Identity

open System.Threading.Tasks
open Microsoft.AspNetCore.Identity
open Storczyk.Prodpol.Core.Models

type EmployeeIdValidator() =
    interface IUserValidator<Employee> with
        member this.ValidateAsync(_, user) =
            task {
                if user.Id >= 1L then
                    return IdentityResult.Success
                else
                    return
                        IdentityResult.Failed(
                            IdentityError(
                                Code = "InvalidEmployeeId",
                                Description = "Employee Id must be greater than or equal to 1"
                            )
                        )
            }
