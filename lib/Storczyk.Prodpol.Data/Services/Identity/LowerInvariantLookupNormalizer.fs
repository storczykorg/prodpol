namespace Storczyk.Prodpol.Data.Services.Identity

open System
open Microsoft.AspNetCore.Identity

type LowerInvariantLookupNormalizer() =
    interface ILookupNormalizer with
        member this.NormalizeEmail(email) =
            if String.IsNullOrWhiteSpace(email) then
                email
            else
                email.ToLowerInvariant()

        member this.NormalizeName(name) =
            if String.IsNullOrWhiteSpace(name) then
                name
            else
                name.ToLowerInvariant()
