namespace Storczyk.Prodpol.Core.Models

open System
open LinqToDB.Mapping
open Storczyk.Prodpol.Core.Utils

[<Serializable; CLIMutable>]
[<ProdpolModel>]
[<Table(Name = "product_descriptions", Schema = "prodpol")>]
type ProductDescription =
    { [<PrimaryKey; Column("product_id")>]
      ProductId: int64

      [<PrimaryKey; Column("language_code")>]
      LanguageCode: string option

      [<Column("title")>]
      Title: string

      [<Column("created_at")>]
      CreatedAt: DateTime

      [<Column("body")>]
      Body: string

      [<Column("is_public")>]
      IsPublic: bool

      [<Column("last_modified_at")>]
      LastModifiedAt: DateTime }
