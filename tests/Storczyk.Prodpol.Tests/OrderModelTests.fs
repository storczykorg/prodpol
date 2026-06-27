module Storczyk.Prodpol.Tests.OrderModelTests

open System
open NUnit.Framework
open Storczyk.Prodpol.Core.Models

[<Test>]
let ``OrderTotalDetails can be constructed`` () =
    let d: OrderTotalDetails =
        { Total = 100m
          DeliveryFee = 15m
          ItemsTotal = 85m }

    Assert.That(d.Total, Is.EqualTo(100m))
    Assert.That(d.DeliveryFee, Is.EqualTo(15m))
    Assert.That(d.ItemsTotal, Is.EqualTo(85m))

[<Test>]
let ``Order can be constructed with all fields`` () =
    let now = DateTime.UtcNow

    let o: Order =
        { Id = 1L
          CustomerId = 10L
          CreatedAt = now
          EmployeeId = 5L
          DeliveryMethod = 2
          Total =
            { Total = 120m
              DeliveryFee = 10m
              ItemsTotal = 110m }
          CurrentState = 3 }

    Assert.That(o.Id, Is.EqualTo(1L))
    Assert.That(o.CustomerId, Is.EqualTo(10L))
    Assert.That(o.Total.Total, Is.EqualTo(120m))
    Assert.That(o.CurrentState, Is.EqualTo(3))

[<Test>]
let ``Order equality works`` () =
    let now = DateTime.UtcNow

    let a: Order =
        { Id = 1L
          CustomerId = 10L
          CreatedAt = now
          EmployeeId = 5L
          DeliveryMethod = 2
          Total =
            { Total = 100m
              DeliveryFee = 10m
              ItemsTotal = 90m }
          CurrentState = 1 }

    let b: Order = a

    Assert.That(a, Is.EqualTo(b))

[<Test>]
let ``OrderRead includes state display fields`` () =
    let now = DateTime.UtcNow

    let r: OrderRead =
        { Id = 1L
          CustomerId = 10L
          CreatedAt = now
          EmployeeId = 5L
          DeliveryMethod = 2
          Total =
            { Total = 120m
              DeliveryFee = 10m
              ItemsTotal = 110m }
          CurrentState = 3
          StateDisplayName = "Shipped"
          StateName = "shipped" }

    Assert.That(r.StateDisplayName, Is.EqualTo("Shipped"))
    Assert.That(r.StateName, Is.EqualTo("shipped"))

[<Test>]
let ``OrderDetails can be constructed with all fields`` () =
    let d: OrderDetails =
        { OrderId = 1L
          FirstName = Some "Jane"
          LastName = Some "Doe"
          CompanyName = Some "Acme"
          NipCode = Some "1234567890"
          City = "Warsaw"
          Street = Some "Main"
          ZipCode = "00-001"
          StreetNo = "10"
          FlatNo = Some "5"
          SpecialInfo = Some "Leave at door"
          PhoneNumber = Some "+48123456789" }

    Assert.That(d.OrderId, Is.EqualTo(1L))
    Assert.That(d.City, Is.EqualTo("Warsaw"))
    Assert.That(d.ZipCode, Is.EqualTo("00-001"))

[<Test>]
let ``OrderDetails required fields work`` () =
    let d: OrderDetails =
        { OrderId = 2L
          FirstName = None
          LastName = None
          CompanyName = None
          NipCode = None
          City = "Krakow"
          Street = None
          ZipCode = "30-001"
          StreetNo = "5"
          FlatNo = None
          SpecialInfo = None
          PhoneNumber = None }

    Assert.That(d.City, Is.EqualTo("Krakow"))
    Assert.That(d.FirstName, Is.EqualTo(None))

[<Test>]
let ``OrderProduct can be constructed`` () =
    let p: OrderProduct =
        { OrderProductId = 1L
          OrderId = 10L
          TotalCost = 50m
          Amount = 2
          Cost = 25m
          ProductId = 100L
          CustomerNotes = Some "Gift wrap" }

    Assert.That(p.OrderProductId, Is.EqualTo(1L))
    Assert.That(p.Amount, Is.EqualTo(2))
    Assert.That(p.CustomerNotes, Is.EqualTo(Some "Gift wrap"))

[<Test>]
let ``OrderProduct CustomerNotes defaults to None`` () =
    let p: OrderProduct =
        { OrderProductId = 2L
          OrderId = 10L
          TotalCost = 30m
          Amount = 1
          Cost = 30m
          ProductId = 101L
          CustomerNotes = None }

    Assert.That(p.CustomerNotes, Is.EqualTo(None))

[<Test>]
let ``OrderOrderKeys GetSqlName returns correct column names`` () =
    Assert.That(OrderUtilHelpers.GetSqlName OrderOrderKeys.OrderId, Is.EqualTo("order_id"))
    Assert.That(OrderUtilHelpers.GetSqlName OrderOrderKeys.State, Is.EqualTo("order_state"))
    Assert.That(OrderUtilHelpers.GetSqlName OrderOrderKeys.Total, Is.EqualTo("order_total"))
    Assert.That(OrderUtilHelpers.GetSqlName OrderOrderKeys.CustomerId, Is.EqualTo("customer_id"))
    Assert.That(OrderUtilHelpers.GetSqlName OrderOrderKeys.EmployeeId, Is.EqualTo("employee_id"))

[<Test>]
let ``OrderOrderKeys GetSqlName throws on unknown value`` () =
    let unknown = enum<OrderOrderKeys> (99)

    Assert.Throws<System.ArgumentException>(Action(fun () -> OrderUtilHelpers.GetSqlName unknown |> ignore))
    |> ignore

[<Test>]
let ``OrderSearchOption has expected defaults`` () =
    let opt = OrderSearchOption()
    Assert.That(opt.limit, Is.EqualTo(20))
    Assert.That(opt.skip, Is.EqualTo(0))
    Assert.That(opt.asc, Is.EqualTo(false))
    Assert.That(opt.employeeIds, Is.EqualTo(None))
    Assert.That(opt.customerIds, Is.EqualTo(None))
    Assert.That(opt.createdFrom, Is.EqualTo(None))
    Assert.That(opt.createdTo, Is.EqualTo(None))
    Assert.That(opt.stateIds, Is.EqualTo(None))
    Assert.That(opt.stateNames, Is.EqualTo(None))

[<Test>]
let ``OrderSearchOption properties are settable`` () =
    let now = DateTime.UtcNow

    let opt =
        OrderSearchOption(
            employeeIds = Some [| 1L; 2L |],
            customerIds = Some [| 10L |],
            createdFrom = Some now,
            createdTo = Some now,
            stateNames = Some [| "shipped" |],
            limit = 5,
            asc = true
        )

    Assert.That(opt.employeeIds, Is.EqualTo(Some [| 1L; 2L |]))
    Assert.That(opt.customerIds, Is.EqualTo(Some [| 10L |]))
    Assert.That(opt.createdFrom, Is.EqualTo(Some now))
    Assert.That(opt.limit, Is.EqualTo(5))
    Assert.That(opt.asc, Is.EqualTo(true))

[<Test>]
let ``OrderSearchResult inherits PersonSearchResult defaults`` () =
    let r = OrderSearchResult()
    Assert.That(r.total, Is.EqualTo(0L))
    Assert.That(r.nextCursor, Is.EqualTo(None))
    Assert.That(r.results |> Seq.length, Is.EqualTo(0))
