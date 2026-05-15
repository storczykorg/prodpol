namespace Storczyk.Prodpol.Core.Data

open System.Collections.Generic
open System.Data
open System.Threading
open System.Threading.Tasks
open FSharp.Control

type RepoResult =
    | Ok
    | Error of DataException

type IRepository<'TKey, 'TValue> =
    interface
        abstract GetAllAsync: token: CancellationToken -> AsyncSeq<'TValue>
        abstract GetByIdAsync: id: 'Tkey -> Async<'TValue option>
        abstract AddAsync: entity: 'TValue -> Async<RepoResult>
        abstract UpdateAsync: key: 'TKey -> entity: 'TValue -> Async<RepoResult>
        abstract DeleteAsync: key: 'TKey -> Async<RepoResult>
    end

type IEmployeesRepository =
    interface
        
    end