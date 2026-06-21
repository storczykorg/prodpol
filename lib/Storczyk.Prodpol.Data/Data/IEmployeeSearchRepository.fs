namespace Storczyk.Prodpol.Core.Data

open System.Threading
open Storczyk.Async
open Storczyk.Prodpol.Core.Models

type IEmployeeSearchRepository =
    interface
        abstract CountSearchAsync:
            options: EmployeeSearchOption * token: CancellationToken ->
                Async<Result<int, DatabaseError>>
        abstract SearchAsync:
            options: EmployeeSearchOption * token: CancellationToken ->
                Async<Result<EmployeeSearchResult, DatabaseError>>
    end