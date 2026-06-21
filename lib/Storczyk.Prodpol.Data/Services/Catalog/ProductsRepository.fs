namespace Storczyk.Prodpol.Data.Services.Catalog

open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models

type ProductsRepository() =
    interface IReadRepository<int64, Product> with
        member this.CountAsync(var0) = failwith "todo"
        member this.GetAllAsync(token) = failwith "todo"
        member this.GetByIdAsync(key) = failwith "todo"