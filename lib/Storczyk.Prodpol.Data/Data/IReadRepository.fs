namespace Storczyk.Prodpol.Core.Data

open System.Threading
open FSharp.Control
open Storczyk.Async
open Storczyk.Prodpol.Core.Models

type IReadRepository<'TKey, 'TValue> =
    interface
        abstract GetAllAsync: token: CancellationToken -> Async<Result<AsyncSeq<'TValue>, DatabaseError>>
        abstract GetByIdAsync: key: 'TKey -> Async<Result<'TValue, DatabaseError>>
        abstract CountAsync: CancellationToken -> Async<Result<int64, DatabaseError>>
    end