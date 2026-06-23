namespace Storczyk.Prodpol.Core.Models

open System
open System.ComponentModel.DataAnnotations
open System.Text.Json.Serialization
open LinqToDB.Mapping
open Storczyk.Prodpol.Core.Utils

[<Serializable; CLIMutable>]
[<ProdpolModel>]
[<Table(Name = "customers", Schema = "prodpol")>]
type Customer =
    { [<PrimaryKey; Column("customer_id")>]
      Id: int64

      [<Column("email")>]
      [<RegularExpression(@"^(([a-zA-Z0-9\-_.+/]+)|(""([+.a-zA-Z0-9_\-]+)""))+@([a-zA-Z]+)(\.([a-zA-Z])+)*$")>]
      [<Required>]
      Email: string

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
      CompanyName: string option }
