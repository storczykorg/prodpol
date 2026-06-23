namespace Storczyk.Prodpol.Core.Data

open System.Threading
open Storczyk.Prodpol.Core.Models

type IEmployeeSearchRepository =
    interface
        abstract CountSearchAsync: options: EmployeeSearchOption * token: CancellationToken -> Async<int>
        abstract SearchAsync: options: EmployeeSearchOption * token: CancellationToken -> Async<EmployeeSearchResult>
    end
