namespace Storczyk.Prodpol.Core.Models

open System
open LinqToDB.Mapping
open Storczyk.Prodpol.Core.Utils

[<Serializable; CLIMutable>]
[<ProdpolModel>]
[<Table(Name = "orders", Schema = "prodpol")>]
type Order =
    { [<PrimaryKey; Column("order_id")>]
      Id: int64

      [<Column("customer_id")>]
      CustomerId: int64

      [<Column("created_at")>]
      CreatedAt: DateTime

      [<Column("employee_id")>]
      EmployeeId: int64

      [<Column("delivery_method")>]
      DeliveryMethod: int

      [<Column("total")>]
      Total: OrderTotalDetails

      [<Column("current_state")>]
      CurrentState: int }
