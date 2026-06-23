namespace Storczyk.Prodpol.Core.Models

open System.Runtime.Serialization
open System.Text.Json.Serialization
open NpgsqlTypes

[<JsonConverter(typeof<JsonStringEnumConverter>)>]
type ProductOrderKeys =
    | [<PgName("product_id"); EnumMember>] ProductId = 0
    | [<PgName("product_name"); EnumMember>] Name = 1
    | [<PgName("product_price"); EnumMember>] Price = 2
    | [<PgName("product_created_at"); EnumMember>] CreatedAt = 3
    | [<PgName("product_available_amount"); EnumMember>] AvailableAmount = 4

module ProductUtilHelpers =
    let GetSqlName (x: ProductOrderKeys) =
        match x with
        | ProductOrderKeys.ProductId -> "product_id"
        | ProductOrderKeys.Name -> "product_name"
        | ProductOrderKeys.Price -> "product_price"
        | ProductOrderKeys.CreatedAt -> "product_created_at"
        | ProductOrderKeys.AvailableAmount -> "product_available_amount"
        | _ -> raise (invalidArg "x" "Unknown enum value")
