namespace Storczyk.Prodpol.Dat.Forms

open System
open System.ComponentModel.DataAnnotations
open Storczyk.Prodpol.Core.Models

[<Serializable>]
type EmployeeCreate() =
    [<RegularExpression("""^(([a-zA-Z0-9\-_.+/]+)|("([+.a-zA-Z0-9_\-]+)"))+@([a-zA-Z]+)(\.([a-zA-Z])+)*$""")>]
    [<Required>]
    member val Email = "" with get, set

    [<RegularExpression("^(\w+\s?)+$")>]
    [<Required>]
    member val NameFirst = "" with get, set

    [<RegularExpression("^(\w+\s?)+$")>]
    [<Required>]
    member val NameLast = "" with get, set

    [<RegularExpression("^\+?[1-9][0-9]{7,14}$")>]
    [<Phone>]
    [<Required>]
    member val PhoneNumber = "" with get, set

    member val Password: string | null = null with get, set

    member this.passwordNotEmpty(): bool =
        this.Password
        |> String.IsNullOrWhiteSpace
        |> not

    member this.BuildEmployee(id: int64, time: DateTime) : Employee =
        Employee(
            Id = id,
            Email = this.Email,
            PhoneNumber = this.PhoneNumber,
            NameFirst = this.NameFirst,
            NameLast = this.NameLast,
            CreatedAt = time
        )
