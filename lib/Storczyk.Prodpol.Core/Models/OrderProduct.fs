namespace Storczyk.Prodpol.Core.Models

open System
open LinqToDB.Mapping
open Storczyk.Prodpol.Core.Utils

[<Serializable; CLIMutable>]
[<ProdpolModel>]
[<Table(Name = "order_products", Schema = "prodpol")>]
type OrderProduct =
    { [<PrimaryKey; Column("order_product_Id")>]
      OrderProductId: int64

      [<Column("order_id")>]
      OrderId: int64

      [<Column("total_cost")>]
      TotalCost: decimal

      [<Column("amount")>]
      Amount: int

      [<Column("cost")>]
      Cost: decimal

      [<Column("product_id")>]
      ProductId: int64

      [<Column("customer_notes")>]
      CustomerNotes: string option }
