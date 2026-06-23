module Storczyk.Prodpol.Tests.ProductModelTests

open System
open NUnit.Framework
open Storczyk.Prodpol.Core.Models

[<Test>]
let ``ProductRead can be constructed with all fields`` () =
    let now = DateTime.UtcNow

    let p: ProductRead =
        { Id = 1L
          Name = "Widget"
          CreatedAt = now
          CreatedBy = 10L
          LastModifiedBy = 10L
          LastModifiedAt = now
          Price = 19.99m
          UnitType = 1
          AvailableAmount = 50
          UnitBase = 1
          LowestPrice = Some 14.99m
          LowestPriceTo = Some now
          LowestPriceFrom = Some now }

    Assert.That(p.Id, Is.EqualTo(1L))
    Assert.That(p.Name, Is.EqualTo("Widget"))
    Assert.That(p.Price, Is.EqualTo(19.99m))
    Assert.That(p.LowestPrice, Is.EqualTo(Some 14.99m))

[<Test>]
let ``ProductRead option fields default to None`` () =
    let now = DateTime.UtcNow

    let p: ProductRead =
        { Id = 2L
          Name = "Gadget"
          CreatedAt = now
          CreatedBy = 1L
          LastModifiedBy = 1L
          LastModifiedAt = now
          Price = 9.99m
          UnitType = 2
          AvailableAmount = 100
          UnitBase = 1
          LowestPrice = None
          LowestPriceTo = None
          LowestPriceFrom = None }

    Assert.That(p.LowestPrice, Is.EqualTo(None))
    Assert.That(p.LowestPriceTo, Is.EqualTo(None))
    Assert.That(p.LowestPriceFrom, Is.EqualTo(None))

[<Test>]
let ``ProductRead equality works`` () =
    let now = DateTime.UtcNow
    let a: ProductRead =
        { Id = 1L; Name = "X"; CreatedAt = now; CreatedBy = 1L
          LastModifiedBy = 1L; LastModifiedAt = now; Price = 5m
          UnitType = 1; AvailableAmount = 10; UnitBase = 1
          LowestPrice = None; LowestPriceTo = None; LowestPriceFrom = None }
    let b: ProductRead =
        { Id = 1L; Name = "X"; CreatedAt = now; CreatedBy = 1L
          LastModifiedBy = 1L; LastModifiedAt = now; Price = 5m
          UnitType = 1; AvailableAmount = 10; UnitBase = 1
          LowestPrice = None; LowestPriceTo = None; LowestPriceFrom = None }

    Assert.That(a, Is.EqualTo(b))

[<Test>]
let ``ProductOrderKeys GetSqlName returns correct column names`` () =
    Assert.That(ProductUtilHelpers.GetSqlName ProductOrderKeys.ProductId, Is.EqualTo("product_id"))
    Assert.That(ProductUtilHelpers.GetSqlName ProductOrderKeys.Name, Is.EqualTo("product_name"))
    Assert.That(ProductUtilHelpers.GetSqlName ProductOrderKeys.Price, Is.EqualTo("product_price"))
    Assert.That(ProductUtilHelpers.GetSqlName ProductOrderKeys.CreatedAt, Is.EqualTo("product_created_at"))
    Assert.That(ProductUtilHelpers.GetSqlName ProductOrderKeys.AvailableAmount, Is.EqualTo("product_available_amount"))

[<Test>]
let ``ProductOrderKeys GetSqlName throws on unknown value`` () =
    let unknown = enum<ProductOrderKeys> (99)

    Assert.Throws<System.ArgumentException>(
        Action(fun () -> ProductUtilHelpers.GetSqlName unknown |> ignore))
    |> ignore

[<Test>]
let ``ProductSearchOption has expected defaults`` () =
    let opt = ProductSearchOption()
    Assert.That(opt.limit, Is.EqualTo(20))
    Assert.That(opt.skip, Is.EqualTo(0))
    Assert.That(opt.asc, Is.EqualTo(false))
    Assert.That(opt.name, Is.EqualTo(None))
    Assert.That(opt.priceMin, Is.EqualTo(None))
    Assert.That(opt.priceMax, Is.EqualTo(None))
    Assert.That(opt.unitType, Is.EqualTo(None))
    Assert.That(opt.orderBy, Is.EqualTo(None))

[<Test>]
let ``ProductSearchOption properties are settable`` () =
    let opt = ProductSearchOption(name = Some "widget", priceMin = Some 10m, priceMax = Some 50m, unitType = Some 1, limit = 10, skip = 5, asc = true)

    Assert.That(opt.name, Is.EqualTo(Some "widget"))
    Assert.That(opt.priceMin, Is.EqualTo(Some 10m))
    Assert.That(opt.priceMax, Is.EqualTo(Some 50m))
    Assert.That(opt.unitType, Is.EqualTo(Some 1))
    Assert.That(opt.limit, Is.EqualTo(10))
    Assert.That(opt.skip, Is.EqualTo(5))
    Assert.That(opt.asc, Is.EqualTo(true))

[<Test>]
let ``ProductSearchResult inherits PersonSearchResult defaults`` () =
    let r = ProductSearchResult()
    Assert.That(r.total, Is.EqualTo(0L))
    Assert.That(r.nextCursor, Is.EqualTo(None))
    Assert.That(r.results |> Seq.length, Is.EqualTo(0))
