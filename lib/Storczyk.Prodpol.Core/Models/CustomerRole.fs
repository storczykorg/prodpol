namespace Storczyk.Prodpol.Core.Models

open System
open LinqToDB.Mapping
open Storczyk.Prodpol.Core.Utils

[<CLIMutable; Serializable>]
[<ProdpolModel>]
[<Table(Name = "customer_roles", Schema = "prodpol")>]
type CustomerRole =
    { [<PrimaryKey; Column("role_id")>]
      Id: int

      [<Column("display_name")>]
      DisplayName: string

      [<Column("role_name")>]
      RoleName: string }
