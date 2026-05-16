namespace Storczyk.Prodpol.Core.Data

open System
open System.Data
open System.Threading
open FSharp.Control
open Storczyk.Prodpol.Core.Models

type ValidationErrorDetail = {
    Field: string
    Issue: string
}

type DatabaseError =
    | NotFound
    | ValidationErrors of ValidationErrorDetail seq // Accumulates multiple validation issues
    | BulkOperationErrors of DatabaseError seq // (Index of row * Error that happened)
    | ConcurrencyViolation
    | ConnectionTimeout
    | DatabaseException of DataException
    | UnknownException of Exception

type IRepository<'TKey, 'TValue> =
    interface
        abstract GetAllAsync: token: CancellationToken -> Async<Result<AsyncSeq<'TValue>, DatabaseError>>
        abstract GetByIdAsync: key: 'TKey -> Async<Result<'TValue, DatabaseError>>
        abstract AddAsync: entity: 'TValue -> Async<Result<unit, DatabaseError>>
        abstract UpdateAsync: key: 'TKey -> entity: 'TValue -> Async<Result<unit, DatabaseError>>
        abstract DeleteAsync: key: 'TKey -> Async<Result<unit, DatabaseError>>
    end

type IEmployeesRepository =
    inherit IRepository<int64, Employee>
    
type IDictionaryRepository<'T> =
    inherit IRepository<string, 'T>

type IEmployeeRoleRepository =
    inherit IDictionaryRepository<EmployeeRole>
    