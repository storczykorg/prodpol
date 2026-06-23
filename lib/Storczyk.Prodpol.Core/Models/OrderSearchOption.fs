namespace Storczyk.Prodpol.Core.Models

open System

type OrderSearchOption() =
    member val employeeIds: int64 array option = None with get, set
    member val customerIds: int64 array option = None with get, set
    member val createdFrom: DateTime option = None with get, set
    member val createdTo: DateTime option = None with get, set
    member val stateIds: int array option = None with get, set
    member val stateNames: string array option = None with get, set
    member val limit: int = 20 with get, set
    member val skip: int = 0 with get, set
    member val orderBy: OrderOrderKeys option = None with get, set
    member val asc: bool = false with get, set
