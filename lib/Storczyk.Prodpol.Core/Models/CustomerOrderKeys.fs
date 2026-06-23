namespace Storczyk.Prodpol.Core.Models

open System.Runtime.Serialization
open System.Text.Json.Serialization
open NpgsqlTypes

[<JsonConverter(typeof<JsonStringEnumConverter>)>]
type CustomerOrderKeys =
    | [<PgName("customer_id"); EnumMember>] CustomerId = 0
    | [<PgName("customer_full_name"); EnumMember>] FullName = 1
    | [<PgName("customer_email"); EnumMember>] Email = 2
    | [<PgName("customer_phone_number"); EnumMember>] PhoneNumber = 3

module CustomerUtilHelpers =
    let GetSqlName (x: CustomerOrderKeys) =
        match x with
        | CustomerOrderKeys.CustomerId -> "customer_id"
        | CustomerOrderKeys.FullName -> "customer_full_name"
        | CustomerOrderKeys.Email -> "customer_email"
        | CustomerOrderKeys.PhoneNumber -> "customer_phone_number"
        | _ -> raise (invalidArg "x" "Unknown enum value")
