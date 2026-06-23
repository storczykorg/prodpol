namespace Storczyk.Prodpol.Core.Models

open System
open System.ComponentModel.DataAnnotations
open System.Text.Json.Serialization
open LinqToDB.Mapping
open Storczyk.Prodpol.Core.Utils

[<Serializable; CLIMutable>]
[<ProdpolModel>]
[<Table(Name = "customers_with_roles", Schema = "prodpol")>]
type CustomerRead =
    { [<PrimaryKey; Column("customer_id")>]
      Id: int64

      [<Column("email")>]
      [<Required>]
      Email: string

      [<Column("normalized_email")>]
      NormalizedEmail: string

      [<Column("phone_number")>]
      PhoneNumber: string option

      [<JsonIgnore>]
      [<Column("password_hash")>]
      PasswordHash: string option

      [<Column("email_confirmed")>]
      EmailConfirmed: bool

      [<Column("role")>]
      Role: int option

      [<Column("name_first")>]
      NameFirst: string option

      [<Column("name_last")>]
      NameLast: string option

      [<Column("company_name")>]
      CompanyName: string option

      [<Column("full_name")>]
      FullName: string

      [<Column("normalized_name")>]
      NormalizedName: string

      [<Column("role_id")>]
      RoleId: int option

      [<Column("display_name")>]
      RoleDisplayName: string option

      [<Column("role_name")>]
      RoleName: string option }
