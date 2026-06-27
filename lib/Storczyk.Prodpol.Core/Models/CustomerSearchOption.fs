namespace Storczyk.Prodpol.Core.Models

type CustomerSearchOption() =
    inherit PersonSearchOption<CustomerOrderKeys>()
    member val companyName: string option = None with get, set
