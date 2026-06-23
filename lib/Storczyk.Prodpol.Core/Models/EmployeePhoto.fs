namespace Storczyk.Prodpol.Core.Models

open System
open LinqToDB.Mapping
open Storczyk.Prodpol.Core.Utils

[<Serializable>]
[<CLIMutable>]
type EmployeePhoto =
    { [<PrimaryKey>]
      [<Column("employee_id")>]
      EmployeeId: int64
      [<Column("mime_type")>]
      MimeType: string
      [<Column("payload")>]
      Payload: byte[]
      [<Column("payload_size")>]
      PayloadSize: int32 }
