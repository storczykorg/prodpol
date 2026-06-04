namespace Storczyk.Prodpol.Core.Models

open System.Runtime.CompilerServices
open System.Runtime.Serialization
open System.Text.Json.Serialization
open NpgsqlTypes

type PersonSearchOption<'TOrder>() =
    member val fullName: string option = None with get, set
    member val email: string option = None with get, set
    member val phoneNumber: string option = None with get, set
    member val limit: int = 20 with get, set
    member val cursor: int64 option = None with get, set
    member val orderBy: 'TOrder option = None with get, set
    member val asc: bool = false with get, set

type PersonSearchResult<'T>() =
    member val total: int64 = 0 with get, set
    member val nextCursor: int64 option = None with get, set
    member val results: 'T seq = [] with get, set

[<JsonConverter(typeof<JsonStringEnumConverter>)>]
type EmployeeOrderKeys =
| [<PgName("employee_id")>] [<EnumMember>] EmployeeId = 0
| [<PgName("employee_email")>] [<EnumMember>] Email = 1
| [<PgName("employee_phone_number")>] [<EnumMember>] PhoneNumber = 2
| [<PgName("employee_full_name")>] [<EnumMember>] FullName = 3

type EmployeeSearchOption() =
    inherit PersonSearchOption<EmployeeOrderKeys>()

type EmployeeSearchResult() =
    inherit PersonSearchResult<EmployeeRead>()
module UtilHelpers =
    let GetSqlName (x: EmployeeOrderKeys) =
        match x with
        | EmployeeOrderKeys.EmployeeId -> "employee_id"
        | EmployeeOrderKeys.Email -> "employee_email"
        | EmployeeOrderKeys.PhoneNumber -> "employee_phone_number"
        | EmployeeOrderKeys.FullName -> "employee_full_name"
        | _ -> failwith "Unknown enum value"
