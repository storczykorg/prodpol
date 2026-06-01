namespace Storczyk.Prodpol.Core.Services

open FSharp.Control
open Storczyk.Prodpol.Core.Data

type NullEmployeeRepository() =
    interface IEmployeesRepository with
        member this.AddAsync(_) = async { return Ok() }
        member this.DeleteAsync(_) = async { return Ok() }
        member this.GetAllAsync(_) = async { return Ok(asyncSeq { }) }
        member this.CountAsync(_) = async.Return(Ok 0L)
        member this.GetByIdAsync(_) = async { return Error(NotFound) }
        member this.UpdateAsync _ _ = async { return Ok() }

type NullEmployeeRoleRepository() =
    interface IEmployeeRoleRepository with
        member this.AddAsync(_) = async { return Ok() }
        member this.DeleteAsync(_) = async { return Ok() }
        member this.GetAllAsync(_) = async { return Ok(asyncSeq { }) }
        member this.CountAsync(_) = async { return Ok 0L }
        member this.GetByIdAsync(_) = async { return Error(NotFound) }
        member this.UpdateAsync _ _ = async { return Ok() }
