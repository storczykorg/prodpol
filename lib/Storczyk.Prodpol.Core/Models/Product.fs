namespace Storczyk.Prodpol.Core.Models

open System
open System.ComponentModel.DataAnnotations
open LinqToDB.Mapping
open Npgsql.TypeMapping
open Storczyk.Prodpol.Core.Utils

[<Serializable; CLIMutable>]
[<ProdpolModel>]
[<Table("prodpol.products")>]
type Product =
    {
        [<PrimaryKey; Column("product_id")>]
        Id: int64

        [<Column("product_name"); MaxLength(255)>]
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

        [<Column("unit_base"); >]
        UnitBase: int
    }