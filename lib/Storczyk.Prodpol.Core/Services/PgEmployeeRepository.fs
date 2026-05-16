namespace Storczyk.Prodpol.Core.Services

open System.Runtime.CompilerServices
open System.Threading
open FSharp.Control
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models

type PgEmployeeRepository() =
    interface IEmployeesRepository with
        member this.AddAsync(entity) = failwith "todo"
        member this.DeleteAsync(key) = failwith "todo"
        member this.GetAllAsync(token) = failwith "todo"
        member this.GetByIdAsync(key) = failwith "todo"
        member this.UpdateAsync key entity = failwith "todo"

type PgEmployeeRoleRepository() =
    interface IEmployeeRoleRepository with
        member this.AddAsync(entity) = failwith "todo"
        member this.DeleteAsync(key) = failwith "todo"
        member this.GetAllAsync(token) = failwith "todo"
        member this.GetByIdAsync(key) = failwith "todo"
        member this.UpdateAsync key entity = failwith "todo"