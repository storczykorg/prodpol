namespace Storczyk.Prodpol.Core.Models

type PersonSearchOption() =
    member val fullName: string option = None with get, set
    member val email: string option = None with get, set
    member val phoneNumber: string option = None with get, set
    member val limit: int = 20 with get, set
    member val cursor: int64 option = None with get, set
    member val orderBy: string option = None with get, set
    member val asc: bool = false with get, set

type PersonSearchResult<'T>() =
    member val total: int64 = 0 with get, set
    member val nextCursor: int64 option = None with get, set
    member val results: 'T seq = [] with get, set

type EmployeeSearchOption() =
    inherit PersonSearchOption()

type EmployeeSearchResult() =
    inherit PersonSearchResult<EmployeeRead>()
