namespace Storczyk.Prodpol.Core.Data

open System.Threading
open FSharp.Control
open Storczyk.Prodpol.Core.Models

type IReadRepository<'TKey, 'TValue> =
    interface
        abstract GetAllAsync: token: CancellationToken -> Async<AsyncSeq<'TValue>>
        abstract GetByIdAsync: key: 'TKey -> Async<'TValue>
        abstract CountAsync: CancellationToken -> Async<int64>
    end
