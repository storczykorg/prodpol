namespace Storczyk.Prodpol.Data.Services.Identity

open System
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open Storczyk.Prodpol.Core.Models

type EmployeeUserManager
    (
        store: IUserStore<Employee>,
        optionsAccessor: IOptions<IdentityOptions>,
        passwordHasher: IPasswordHasher<Employee>,
        userValidators: IEnumerable<IUserValidator<Employee>>,
        passwordValidators: IEnumerable<IPasswordValidator<Employee>>,
        keyNormalizer: ILookupNormalizer,
        errors: IdentityErrorDescriber,
        services: IServiceProvider,
        logger: ILogger<UserManager<Employee>>
    ) =
    inherit
        UserManager<Employee>(
            store,
            optionsAccessor,
            passwordHasher,
            userValidators,
            passwordValidators,
            keyNormalizer,
            errors,
            services,
            logger
        )

    member private this.DoVerifyUserToken(user, tokenProvider, purpose, token) =
        this.VerifyUserTokenAsync(user, tokenProvider, purpose, token)

    member private this.DoUpdatePasswordHash(user, newPassword) =
        this.UpdatePasswordHash(user, newPassword, true)

    override this.ChangePasswordAsync(user, currentPassword, newPassword) =
        let passwordStore = store :?> IUserPasswordStore<Employee>

        task {
            let! currentHash = passwordStore.GetPasswordHashAsync(user, CancellationToken.None)

            let verifyResult =
                passwordHasher.VerifyHashedPassword(user, currentHash, currentPassword)

            if verifyResult <> PasswordVerificationResult.Failed then
                return! this.DoUpdatePasswordHash(user, newPassword)
            else
                return IdentityResult.Failed(errors.PasswordMismatch())
        }

    override this.ResetPasswordAsync(user, token, newPassword) =
        task {
            let provider = this.Options.Tokens.PasswordResetTokenProvider

            let! isValid =
                this.DoVerifyUserToken(user, provider, UserManager<Employee>.ResetPasswordTokenPurpose, token)

            if not isValid then
                return IdentityResult.Failed(errors.InvalidToken())
            else
                return! this.DoUpdatePasswordHash(user, newPassword)
        }

    override this.ConfirmEmailAsync(user, token) =
        task {
            let provider = this.Options.Tokens.EmailConfirmationTokenProvider
            let purpose = UserManager<Employee>.ConfirmEmailTokenPurpose
            let! isValid = this.DoVerifyUserToken(user, provider, purpose, token)

            if not isValid then
                return IdentityResult.Failed(errors.InvalidToken())
            else
                let emailStore = store :?> IUserEmailStore<Employee>
                do! emailStore.SetEmailConfirmedAsync(user, true, CancellationToken.None)
                return! this.UpdateSecurityStampAsync(user)
        }
