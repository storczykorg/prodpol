namespace Storczyk.Prodpol.Core.Data

open System.Threading
open FSharp.Control
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Data.Models

type IRepository<'TKey, 'TValue> =
    interface
        inherit IReadRepository<'TKey, 'TValue>
        abstract AddAsync: entity: 'TValue -> Async<unit>
        abstract UpdateAsync: key: 'TKey * entity: 'TValue -> Async<unit>
        abstract DeleteAsync: key: 'TKey -> Async<unit>
    end

type IEmployeesRepository =
    inherit IRepository<int64, Employee>

type IDictionaryRepository<'T> =
    inherit IRepository<string, 'T>

type IEmployeeRoleRepository =
    inherit IDictionaryRepository<EmployeeRole>

type IEmployeesReadRepository =
    inherit IReadRepository<int64, EmployeeRead>

type IEmployeeRoleReadRepository =
    inherit IReadRepository<string, EmployeeRoleRead>

type ICustomerSpendingReadRepository =
    inherit IReadRepository<int64, CustomerSpending>
