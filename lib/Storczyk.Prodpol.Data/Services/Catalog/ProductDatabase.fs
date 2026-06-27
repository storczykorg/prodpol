namespace Storczyk.Prodpol.Data.Services.Catalog

open LinqToDB
open Storczyk.Prodpol.Core.Models

type IProductDatabase =
    interface
        inherit IDataContext
        abstract member ProductRead: ITable<ProductRead>
    end

type ProductDatabase(options: DataOptions) =
    inherit LinqToDB.DataContext(options)

    new() = ProductDatabase(DataOptions())

    interface IProductDatabase with
        member this.ProductRead = this.GetTable<ProductRead>()
