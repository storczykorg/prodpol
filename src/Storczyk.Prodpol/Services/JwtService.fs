namespace Storczyk.Prodpol.Services

open System
open System.Collections.Concurrent
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Security.Cryptography
open Microsoft.Extensions.Options
open Microsoft.IdentityModel.Tokens

type JwtOptions() =
    member val Issuer = "" with get, set
    member val Audience = "" with get, set
    member val AccessTokenExpireMinutes = 15.0 with get, set
    member val RefreshTokenExpireDays = 7.0 with get, set

type JwtLoginRequest = { Email: string; Password: string }

type JwtTokenResponse =
    { AccessToken: string
      RefreshToken: string
      ExpiresIn: float }

type JwtRefreshRequest = { RefreshToken: string }

type JwtService(options: IOptions<JwtOptions>) =
    let ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256)
    let signingKey = ECDsaSecurityKey(ecdsa)

    let signingCredentials =
        SigningCredentials(signingKey, SecurityAlgorithms.EcdsaSha256)

    let handler = JwtSecurityTokenHandler()
    let refreshTokens = ConcurrentDictionary<string, int64 * DateTime>()

    member this.GetValidationParameters() =
        let opts = options.Value
        let validateIssuer = not (String.IsNullOrEmpty(opts.Issuer))
        let validateAudience = not (String.IsNullOrEmpty(opts.Audience))

        TokenValidationParameters(
            ValidateIssuer = validateIssuer,
            ValidIssuer = opts.Issuer,
            ValidateAudience = validateAudience,
            ValidAudience = opts.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        )

    member this.GenerateAccessToken(claims: seq<Claim>) : string =
        let now = DateTime.UtcNow

        let token =
            JwtSecurityToken(
                issuer = options.Value.Issuer,
                audience = options.Value.Audience,
                claims = claims,
                notBefore = now,
                expires = now.AddMinutes(options.Value.AccessTokenExpireMinutes),
                signingCredentials = signingCredentials
            )

        handler.WriteToken(token)

    member this.GenerateRefreshToken(userId: int64) : string =
        let tokenBytes = RandomNumberGenerator.GetBytes(32)
        let token = Convert.ToBase64String(tokenBytes)
        let expires = DateTime.UtcNow.AddDays(options.Value.RefreshTokenExpireDays)
        refreshTokens.[token] <- (userId, expires)
        token

    member this.TryValidateRefreshToken(token: string) : int64 option =
        match refreshTokens.TryGetValue(token) with
        | true, (userId, expires) when expires > DateTime.UtcNow ->
            refreshTokens.TryRemove(token) |> ignore
            Some userId
        | _ -> None
