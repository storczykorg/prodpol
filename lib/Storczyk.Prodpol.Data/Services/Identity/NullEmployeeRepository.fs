namespace Storczyk.Prodpol.Core.Services

open FSharp.Control
open Storczyk.Async
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Utils

type NullEmployeeRepository() =
    interface IEmployeesRepository with
        member this.AddAsync(_) = async { return () }
        member this.DeleteAsync(_) = async { return () }
        member this.GetAllAsync(_) = async { return asyncSeq { } }
        member this.CountAsync(_) = async { return 0L }
        member this.GetByIdAsync(_) = async { return raise (NotFoundException "Resource not found.") }
        member this.UpdateAsync (_, _) = async { return () }

type NullEmployeeRoleRepository() =
    interface IEmployeeRoleRepository with
        member this.AddAsync(_) = async { return () }
        member this.DeleteAsync(_) = async { return () }
        member this.GetAllAsync(_) = async { return asyncSeq { } }
        member this.CountAsync(_) = async { return 0L }
        member this.GetByIdAsync(_) = async { return raise (NotFoundException "Resource not found.") }
        member this.UpdateAsync (_, _) = async { return () }
