namespace Storczyk.Prodpol.Core.Services

open System
open System.Linq
open System.Threading
open LinqToDB
open LinqToDB.Async
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Utils
open Storczyk.Prodpol.Data.Services.Catalog

[<RegisterAsTransient(typeof<IProductSearchRepository>)>]
type PgProductSearchRepository(db: IProductDatabase) =

    let buildFilteredQuery (name: string) (priceMin: decimal) (priceMax: decimal) (unitType: int) =
        query {
            for p in db.ProductRead do
                where (name = "" || p.Name.Contains(name))
                where (priceMin = 0m || p.Price >= priceMin)
                where (priceMax = 0m || p.Price <= priceMax)
                where (unitType = 0 || p.UnitType = unitType)
                select p
        }

    let buildOrderedQuery (q: IQueryable<ProductRead>) (key: ProductOrderKeys) (asc: bool) =
        match key, asc with
        | ProductOrderKeys.Name, true -> q.OrderBy(fun p -> p.Name).ThenBy(fun p -> p.Id)
        | ProductOrderKeys.Name, false -> q.OrderByDescending(fun p -> p.Name).ThenByDescending(fun p -> p.Id)
        | ProductOrderKeys.Price, true -> q.OrderBy(fun p -> p.Price).ThenBy(fun p -> p.Id)
        | ProductOrderKeys.Price, false -> q.OrderByDescending(fun p -> p.Price).ThenByDescending(fun p -> p.Id)
        | ProductOrderKeys.CreatedAt, true -> q.OrderBy(fun p -> p.CreatedAt).ThenBy(fun p -> p.Id)
        | ProductOrderKeys.CreatedAt, false -> q.OrderByDescending(fun p -> p.CreatedAt).ThenByDescending(fun p -> p.Id)
        | ProductOrderKeys.AvailableAmount, true -> q.OrderBy(fun p -> p.AvailableAmount).ThenBy(fun p -> p.Id)
        | ProductOrderKeys.AvailableAmount, false ->
            q.OrderByDescending(fun p -> p.AvailableAmount).ThenByDescending(fun p -> p.Id)
        | _, true -> q.OrderBy(fun p -> p.Id).ThenBy(fun p -> p.Id)
        | _, false -> q.OrderByDescending(fun p -> p.Id).ThenByDescending(fun p -> p.Id)

    let countResults (q: IQueryable<ProductRead>) (token: CancellationToken) =
        async {
            let! count = q.LongCountAsync(token) |> Async.AwaitTask
            return int count
        }

    interface IProductSearchRepository with
        member this.CountSearchAsync(options: ProductSearchOption, token) =
            async {
                let name = options.name |> Option.defaultValue ""
                let priceMin = options.priceMin |> Option.defaultValue 0m
                let priceMax = options.priceMax |> Option.defaultValue 0m
                let unitType = options.unitType |> Option.defaultValue 0

                return! countResults (buildFilteredQuery name priceMin priceMax unitType) token
            }

        member this.SearchAsync(options: ProductSearchOption, token) =
            async {
                let name = options.name |> Option.defaultValue ""
                let priceMin = options.priceMin |> Option.defaultValue 0m
                let priceMax = options.priceMax |> Option.defaultValue 0m
                let unitType = options.unitType |> Option.defaultValue 0
                let orderKey = options.orderBy |> Option.defaultValue ProductOrderKeys.ProductId

                let filtered = buildFilteredQuery name priceMin priceMax unitType
                let ordered = buildOrderedQuery filtered orderKey options.asc

                let! page =
                    ordered.Skip(options.skip).Take(options.limit).ToArrayAsync(token)
                    |> Async.AwaitTask

                let! total = countResults filtered token

                let nextCursor =
                    if page.Length > 0 && int64 options.skip + page.LongLength < total then
                        Some(page.LongLength + int64 options.skip)
                    else
                        None

                return ProductSearchResult(results = (page :> seq<_>), nextCursor = nextCursor, total = total)
            }
