namespace Storczyk.Prodpol.Core.Models

open System
open LinqToDB.Mapping
open Storczyk.Prodpol.Core.Utils

[<Serializable; CLIMutable>]
type OrderTotalDetails =
    { [<Column("orders_total")>]
      Total: decimal

      [<Column("delivery_fee")>]
      DeliveryFee: decimal

      [<Column("items_total")>]
      ItemsTotal: decimal }
