module Storczyk.Prodpol.Tests.CustomerModelTests

open System
open NUnit.Framework
open Storczyk.Prodpol.Core.Models

[<Test>]
let ``Customer can be constructed with all fields`` () =
    let c: Customer =
        { Id = 1L
          Email = "test@example.com"
          PhoneNumber = Some "+48123456789"
          PasswordHash = Some "hash"
          EmailConfirmed = true
          Role = Some 2
          NameFirst = Some "Jane"
          NameLast = Some "Doe"
          CompanyName = Some "Acme" }

    Assert.That(c.Id, Is.EqualTo(1L))
    Assert.That(c.Email, Is.EqualTo("test@example.com"))
    Assert.That(c.EmailConfirmed, Is.EqualTo(true))
    Assert.That(c.NameFirst, Is.EqualTo(Some "Jane"))

[<Test>]
let ``Customer option fields are None and EmailConfirmed is false by default`` () =
    let c: Customer =
        { Id = 2L
          Email = "null@example.com"
          PhoneNumber = None
          PasswordHash = None
          EmailConfirmed = false
          Role = None
          NameFirst = None
          NameLast = None
          CompanyName = None }

    Assert.That(c.PhoneNumber, Is.EqualTo(None))
    Assert.That(c.PasswordHash, Is.EqualTo(None))
    Assert.That(c.Role, Is.EqualTo(None))
    Assert.That(c.NameFirst, Is.EqualTo(None))
    Assert.That(c.CompanyName, Is.EqualTo(None))
    Assert.That(c.EmailConfirmed, Is.EqualTo(false))

[<Test>]
let ``CustomerRead can be constructed with all fields including generated columns`` () =
    let r: CustomerRead =
        { Id = 1L
          Email = "test@example.com"
          NormalizedEmail = "test@example.com"
          PhoneNumber = Some "+48123456789"
          PasswordHash = Some "hash"
          EmailConfirmed = true
          Role = Some 2
          NameFirst = Some "Jane"
          NameLast = Some "Doe"
          CompanyName = Some "Acme"
          FullName = "Jane Doe"
          NormalizedName = "jane doe"
          RoleId = Some 2
          RoleDisplayName = Some "Customer"
          RoleName = Some "customer" }

    Assert.That(r.FullName, Is.EqualTo("Jane Doe"))
    Assert.That(r.NormalizedEmail, Is.EqualTo("test@example.com"))
    Assert.That(r.RoleName, Is.EqualTo(Some "customer"))

[<Test>]
let ``CustomerRead equality works`` () =
    let a: CustomerRead =
        { Id = 1L
          Email = "a@b.com"
          NormalizedEmail = "a@b.com"
          PhoneNumber = None
          PasswordHash = None
          EmailConfirmed = false
          Role = None
          NameFirst = None
          NameLast = None
          CompanyName = None
          FullName = ""
          NormalizedName = ""
          RoleId = None
          RoleDisplayName = None
          RoleName = None }

    let b: CustomerRead = a

    Assert.That(a, Is.EqualTo(b))

[<Test>]
let ``CustomerRole can be constructed`` () =
    let r: CustomerRole =
        { Id = 1
          DisplayName = "Standard Customer"
          RoleName = "standard" }

    Assert.That(r.Id, Is.EqualTo(1))
    Assert.That(r.DisplayName, Is.EqualTo("Standard Customer"))
    Assert.That(r.RoleName, Is.EqualTo("standard"))

[<Test>]
let ``CustomerOrderKeys GetSqlName returns correct column names`` () =
    Assert.That(CustomerUtilHelpers.GetSqlName CustomerOrderKeys.CustomerId, Is.EqualTo("customer_id"))
    Assert.That(CustomerUtilHelpers.GetSqlName CustomerOrderKeys.FullName, Is.EqualTo("customer_full_name"))
    Assert.That(CustomerUtilHelpers.GetSqlName CustomerOrderKeys.Email, Is.EqualTo("customer_email"))
    Assert.That(CustomerUtilHelpers.GetSqlName CustomerOrderKeys.PhoneNumber, Is.EqualTo("customer_phone_number"))

[<Test>]
let ``CustomerOrderKeys GetSqlName throws on unknown value`` () =
    let unknown = enum<CustomerOrderKeys> (99)

    Assert.Throws<System.ArgumentException>(Action(fun () -> CustomerUtilHelpers.GetSqlName unknown |> ignore))
    |> ignore

[<Test>]
let ``CustomerSearchOption inherits PersonSearchOption properties`` () =
    let opt =
        CustomerSearchOption(fullName = Some "Jane", email = Some "j@b.com", limit = 5, asc = true)

    Assert.That(opt.fullName, Is.EqualTo(Some "Jane"))
    Assert.That(opt.email, Is.EqualTo(Some "j@b.com"))
    Assert.That(opt.limit, Is.EqualTo(5))
    Assert.That(opt.asc, Is.EqualTo(true))
    Assert.That(opt.skip, Is.EqualTo(0))
    Assert.That(opt.orderBy, Is.EqualTo(None))

[<Test>]
let ``CustomerSearchResult inherits PersonSearchResult defaults`` () =
    let r = CustomerSearchResult()
    Assert.That(r.total, Is.EqualTo(0L))
    Assert.That(r.nextCursor, Is.EqualTo(None))
    Assert.That(r.results |> Seq.length, Is.EqualTo(0))
