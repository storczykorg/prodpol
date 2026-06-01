namespace Storczyk.Prodpol.Core.Data

open System.Threading
open FSharp.Control
open Storczyk.Prodpol.Core.Models
type IReadRepository<'TKey, 'TValue> =
    interface
        abstract GetAllAsync: token: CancellationToken -> Async<Result<AsyncSeq<'TValue>, DatabaseError>>
        abstract CountAsync: token: CancellationToken -> Async<Result<int64, DatabaseError>>
        abstract GetByIdAsync: key: 'TKey -> Async<Result<'TValue, DatabaseError>>
    end

type IRepository<'TKey, 'TValue> =
    interface
        inherit IReadRepository<'TKey, 'TValue>
        abstract AddAsync: entity: 'TValue -> Async<Result<unit, DatabaseError>>
        abstract UpdateAsync: key: 'TKey -> entity: 'TValue -> Async<Result<unit, DatabaseError>>
        abstract DeleteAsync: key: 'TKey -> Async<Result<unit, DatabaseError>>
    end

type IEmployeesRepository =
    inherit IRepository<int64, Employee>
type IEmployeesReadRepository =
    inherit IReadRepository<int64, EmployeeRead>

type IDictionaryRepository<'T> =
    inherit IRepository<string, 'T>

type IEmployeeRoleRepository =
    inherit IDictionaryRepository<EmployeeRole>
