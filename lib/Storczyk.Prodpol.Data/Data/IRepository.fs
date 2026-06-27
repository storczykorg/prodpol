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

type IProductRepository =
    inherit IRepository<int64, Product>

type IProductReadRepository =
    inherit IReadRepository<int64, ProductRead>

type IProductDescriptionRepository =
    inherit IRepository<int64 * string option, ProductDescription>

type ICustomerRepository =
    inherit IRepository<int64, Customer>

type ICustomerReadRepository =
    inherit IReadRepository<int64, CustomerRead>

type ICustomerRoleRepository =
    inherit IDictionaryRepository<CustomerRole>

type IOrderRepository =
    inherit IRepository<int64, Order>

type IOrderReadRepository =
    inherit IReadRepository<int64, OrderRead>

type IOrderDetailsRepository =
    inherit IRepository<int64, OrderDetails>

type IOrderProductRepository =
    inherit IRepository<int64, OrderProduct>
