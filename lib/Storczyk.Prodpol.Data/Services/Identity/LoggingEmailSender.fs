namespace Storczyk.Prodpol.Core.Services

open System.Threading.Tasks
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.Logging
open Storczyk.Prodpol.Core.Models

type LoggingEmailSender(logger: ILogger<LoggingEmailSender>) =
    interface IEmailSender<Employee> with
        member this.SendConfirmationLinkAsync(user, email, confirmationLink) =
            task { logger.LogInformation("Email confirmation link for {Email}: {Link}", email, confirmationLink) }

        member this.SendPasswordResetLinkAsync(user, email, resetLink) =
            task { logger.LogInformation("Password reset link for {Email}: {Link}", email, resetLink) }

        member this.SendPasswordResetCodeAsync(user, email, resetCode) =
            task { logger.LogInformation("Password reset code for {Email}: {Code}", email, resetCode) }
