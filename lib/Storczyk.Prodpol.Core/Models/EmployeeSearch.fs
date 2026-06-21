namespace Storczyk.Prodpol.Core.Models

open System.Runtime.Serialization
open System.Text.Json.Serialization
open NpgsqlTypes
open Storczyk.Prodpol.Core.Utils

/// Represents search options for querying persons, allowing customization of filter criteria,
/// pagination, and ordering.
///
/// The type parameter 'TOrder specifies the type of the ordering keys.
type PersonSearchOption<'TOrder>() =
    member val fullName: string option = None with get, set
    member val email: string option = None with get, set
    member val phoneNumber: string option = None with get, set
    member val limit: int = 20 with get, set
    member val skip: int = 0 with get, set
    member val orderBy: 'TOrder option = None with get, set
    member val roleNames: string array option = None with get, set
    member val asc: bool = false with get, set

/// Represents the result of a search operation for persons, including the total count of matching
/// entries, the cursor for the next page of results if available, and the sequence of results.
///
/// The type parameter 'T specifies the type of the individual search results.
type PersonSearchResult<'T>() =
    member val total: int64 = 0 with get, set
    member val nextCursor: int64 option = None with get, set
    member val results: 'T seq = [] with get, set

/// Represents the ordering keys for sorting employees in search operations or queries.
/// Each value corresponds to a specific database column used for ordering.
[<JsonConverter(typeof<JsonStringEnumConverter>)>]
type EmployeeOrderKeys =
    | [<PgName("employee_id"); EnumMember>] EmployeeId = 0
    | [<PgName("employee_email"); EnumMember>] Email = 1
    | [<PgName("employee_phone_number"); EnumMember>] PhoneNumber = 2
    | [<PgName("employee_full_name"); EnumMember>] FullName = 3
    | [<PgName("employee_role_name"); EnumMember>] RoleName = 4

type EmployeeSearchOption() =
    inherit PersonSearchOption<EmployeeOrderKeys>()

type EmployeeSearchResult() =
    inherit PersonSearchResult<EmployeeRead>()

/// Provides utility functions for mapping enumeration values to their SQL database column names.
/// This module includes helpers to simplify operations related to SQL column identification
/// based on predefined enumerations.
module UtilHelpers =
    /// Maps an enumeration value of type `EmployeeOrderKeys` to its corresponding SQL database column name.
    ///
    /// Parameters:
    /// - `x`: The `EmployeeOrderKeys` enumeration value to be converted.
    ///
    /// Returns:
    /// A string representing the SQL column name that corresponds to the provided `EmployeeOrderKeys` enumeration value.
    ///
    /// Throws:
    /// - `System.ArgumentException`: Thrown if the provided enumeration value does not match any known case.
    let GetSqlName (x: EmployeeOrderKeys) =
        match x with
        | EmployeeOrderKeys.EmployeeId -> "employee_id"
        | EmployeeOrderKeys.Email -> "employee_email"
        | EmployeeOrderKeys.PhoneNumber -> "employee_phone_number"
        | EmployeeOrderKeys.FullName -> "employee_full_name"
        | EmployeeOrderKeys.RoleName -> "employee_role_name"
        | _ -> raise (invalidArg "x" "Unknown enum value")
