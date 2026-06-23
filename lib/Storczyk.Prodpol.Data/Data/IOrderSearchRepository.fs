namespace Storczyk.Prodpol.Core.Data

open System.Threading
open Storczyk.Prodpol.Core.Models

type IOrderSearchRepository =
    interface
        abstract CountSearchAsync: options: OrderSearchOption * token: CancellationToken -> Async<int>
        abstract SearchAsync: options: OrderSearchOption * token: CancellationToken -> Async<OrderSearchResult>
    end
