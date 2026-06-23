namespace Storczyk.Prodpol.Core.Models

open System
open LinqToDB.Mapping
open Storczyk.Prodpol.Core.Utils

[<Serializable; CLIMutable>]
[<ProdpolModel>]
[<Table(Name = "products_omnibus", Schema = "prodpol")>]
type ProductRead =
    { [<PrimaryKey; Column("product_id")>]
      Id: int64

      [<Column("product_name")>]
      Name: string

      [<Column("created_at")>]
      CreatedAt: DateTime

      [<Column("created_by")>]
      CreatedBy: int64

      [<Column("last_modified_by")>]
      LastModifiedBy: int64

      [<Column("last_modified_at")>]
      LastModifiedAt: DateTime

      [<Column("price")>]
      Price: decimal

      [<Column("unit_type")>]
      UnitType: int

      [<Column("available_amount")>]
      AvailableAmount: int

      [<Column("unit_base")>]
      UnitBase: int

      [<Column("lowest_price")>]
      LowestPrice: decimal option

      [<Column("lowest_price_to")>]
      LowestPriceTo: DateTime option

      [<Column("lowest_price_from")>]
      LowestPriceFrom: DateTime option }
