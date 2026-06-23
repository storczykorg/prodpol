namespace Storczyk.Prodpol.Core.Data

open System.Threading
open Storczyk.Prodpol.Core.Models

type ICustomerSearchRepository =
    interface
        abstract CountSearchAsync: options: CustomerSearchOption * token: CancellationToken -> Async<int>
        abstract SearchAsync: options: CustomerSearchOption * token: CancellationToken -> Async<CustomerSearchResult>
    end
