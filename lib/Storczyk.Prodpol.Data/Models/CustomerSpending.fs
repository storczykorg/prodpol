namespace Storczyk.Prodpol.Data.Models

open System
open LinqToDB.Mapping
open Storczyk.Prodpol.Core.Utils

[<CLIMutable; Serializable>]
[<ProdpolModel>]
type CustomerSpending =
    { [<PrimaryKey; Column("customer_id")>]
      CustomerId: int64
      [<Column("full_name")>]
      FullName: string
      [<Column("email")>]
      Email: string
      [<Column("total_orders")>]
      TotalOrders: int
      [<Column("total_spent")>]
      TotalSpent: decimal
      [<Column("average_order_value")>]
      AverageOrderValue: double
      [<Column("p25")>]
      P25: double
      [<Column("p50")>]
      P50: double
      [<Column("p75")>]
      P75: double
      [<Column("most_expensive_order")>]
      MostExpensiveOrder: double
      [<Column("last_order_date")>]
      LastOrderDate: DateTime }
