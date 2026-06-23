namespace Storczyk.Prodpol.Core.Data

open System.Threading
open Storczyk.Prodpol.Core.Models

type IProductSearchRepository =
    interface
        abstract CountSearchAsync: options: ProductSearchOption * token: CancellationToken -> Async<int>
        abstract SearchAsync: options: ProductSearchOption * token: CancellationToken -> Async<ProductSearchResult>
    end
