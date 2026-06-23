namespace Storczyk.Prodpol.Core.Models

open System
open LinqToDB.Mapping
open Storczyk.Prodpol.Core.Utils

[<Serializable; CLIMutable>]
[<ProdpolModel>]
[<Table(Name = "order_details", Schema = "prodpol")>]
type OrderDetails =
    { [<PrimaryKey; Column("order_id")>]
      OrderId: int64

      [<Column("first_name")>]
      FirstName: string option

      [<Column("last_name")>]
      LastName: string option

      [<Column("company_name")>]
      CompanyName: string option

      [<Column("NIP_code")>]
      NipCode: string option

      [<Column("city")>]
      City: string

      [<Column("street")>]
      Street: string option

      [<Column("zip_code")>]
      ZipCode: string

      [<Column("street_no")>]
      StreetNo: string

      [<Column("flat_no")>]
      FlatNo: string option

      [<Column("special_info")>]
      SpecialInfo: string option

      [<Column("phone_number")>]
      PhoneNumber: string option }
