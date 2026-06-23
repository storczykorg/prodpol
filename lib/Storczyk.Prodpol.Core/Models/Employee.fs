namespace Storczyk.Prodpol.Core.Models

open System
open System.ComponentModel.DataAnnotations
open System.Text.Json.Serialization
open LinqToDB.Mapping
open Storczyk.Prodpol.Core.Utils

[<Serializable; ProdpolModel>]

type ReadonlyEmployee() =
    [<Column("employee_id")>]
    [<Required>]
    [<Identity>]
    member val Id = -1L with get

    [<Column("role_id")>]
    member val RoleId: int option = None with get

    [<Column("email")>]
    [<RegularExpression("""^(([a-zA-Z0-9\-_.+/]+)|("([+.a-zA-Z0-9_\-]+)"))+@([a-zA-Z]+)(\.([a-zA-Z])+)*$""")>]
    [<Required>]
    member val Email = "" with get

    [<Column("email_confirmed")>]
    member val EmailConfirmed = false with get

    [<Column("name_first")>]
    [<RegularExpression("^(\w+\s?)+$")>]
    [<Required>]
    member val NameFirst = "" with get

    [<Column("name_last")>]
    [<RegularExpression("^(\w+\s?)+$")>]
    [<Required>]
    member val NameLast = "" with get

    [<Column("phone_number")>]
    [<MinLength(1)>]
    [<RegularExpression("^\+?[1-9][0-9]{7,14}$")>]
    [<Required>]
    member val PhoneNumber = "" with get

    [<Column("password_hash")>]
    [<JsonIgnore>]
    member val PasswordHash: string option = None with get

    [<Column("security_stamp")>]
    member val SecurityStamp: string option = None with get

    [<Column("created_at")>]
    member val CreatedAt = DateTime.UtcNow with get

    [<Column("enabled")>]
    member val Enabled = false with get

    override this.ToString() = Json.readableJson this

[<Serializable; ProdpolModel>]
[<Table(Name = "employees", Schema = "prodpol")>]
type Employee() =
    [<Column("employee_id")>]
    [<Required>]
    [<Identity>]
    member val Id = -1L with get, set

    [<Column("role_id")>]
    member val RoleId: int option = None with get, set

    [<Column("email")>]
    [<RegularExpression("""^(([a-zA-Z0-9\-_.+/]+)|("([+.a-zA-Z0-9_\-]+)"))+@([a-zA-Z]+)(\.([a-zA-Z])+)*$""")>]
    [<Required>]
    member val Email = "" with get, set

    [<Column("email_confirmed")>]
    member val EmailConfirmed = false with get, set

    [<Column("normalized_email")>]
    member val NormalizedEmail: string option = None with get, set

    [<Column("name_first")>]
    [<RegularExpression("^(\w+\s?)+$")>]
    [<Required>]
    member val NameFirst = "" with get, set

    [<Column("name_last")>]
    [<RegularExpression("^(\w+\s?)+$")>]
    [<Required>]
    member val NameLast = "" with get, set

    [<Column("phone_number")>]
    [<MinLength(1)>]
    [<RegularExpression("^\+?[1-9][0-9]{7,14}$")>]
    [<Required>]
    member val PhoneNumber = "" with get, set

    [<Column("password_hash")>]
    [<JsonIgnore>]
    member val PasswordHash: string option = None with get, set

    [<Column("security_stamp")>]
    member val SecurityStamp: string option = None with get, set

    [<Column("created_at")>]
    member val CreatedAt = DateTime.UtcNow with get, set

    [<Column("enabled")>]
    member val Enabled = false with get, set

    override this.ToString() = Json.readableJson this

/// <summary>
/// Extended version of <see cref="Employee"/> that includes computed columns.
/// Use only on readonly operations
/// </summary>
[<Table(Name = "employees_with_roles", Schema = "prodpol")>]
[<Serializable; ProdpolModel>]
type EmployeeRead() =
    inherit Employee()

    [<Column("email_confirmed")>]
    member val EmailConfirmed = false with get, set

    [<Column("normalized_email")>]
    member val NormalizedEmail = "" with get, set

    [<Column("full_name")>]
    member val FullName = "" with get, set

    [<Column("normalized_name")>]
    member val NormalizedName = "" with get, set

    [<Column("role_name")>]
    member val RoleName: string option = None with get, set

    [<Column("role_display_name")>]
    member val RoleDisplayName: string option = None with get, set

    override this.ToString() = Json.readableJson this
