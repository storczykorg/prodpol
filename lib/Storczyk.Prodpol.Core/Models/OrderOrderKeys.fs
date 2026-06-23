namespace Storczyk.Prodpol.Core.Models

open System.Runtime.Serialization
open System.Text.Json.Serialization
open NpgsqlTypes

[<JsonConverter(typeof<JsonStringEnumConverter>)>]
type OrderOrderKeys =
    | [<PgName("order_id"); EnumMember>] OrderId = 0
    | [<PgName("order_state"); EnumMember>] State = 1
    | [<PgName("order_total"); EnumMember>] Total = 2
    | [<PgName("customer_id"); EnumMember>] CustomerId = 3
    | [<PgName("employee_id"); EnumMember>] EmployeeId = 4

module OrderUtilHelpers =
    let GetSqlName (x: OrderOrderKeys) =
        match x with
        | OrderOrderKeys.OrderId -> "order_id"
        | OrderOrderKeys.State -> "order_state"
        | OrderOrderKeys.Total -> "order_total"
        | OrderOrderKeys.CustomerId -> "customer_id"
        | OrderOrderKeys.EmployeeId -> "employee_id"
        | _ -> raise (invalidArg "x" "Unknown enum value")
