namespace Storczyk.Prodpol.Core.Models

open System
open System.ComponentModel.DataAnnotations.Schema

[<Table("prodpol.employees")>]
type Employee() =
    member val Id = -1l with get, set
    member val RoleId = -1 with get, set
    member val Email = "" with get, set
    member val NameFirst = "" with get, set
    member val NameLast = "" with get, set
    member val PhoneNumber = "" with get, set
    member val PasswordHash: string option = None with get, set
    member val CreatedAt = DateTime.UtcNow with get, set
    member val Enabled = false with get, set