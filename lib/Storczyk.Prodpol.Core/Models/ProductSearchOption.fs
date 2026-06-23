namespace Storczyk.Prodpol.Core.Models

type ProductSearchOption() =
    member val name: string option = None with get, set
    member val priceMin: decimal option = None with get, set
    member val priceMax: decimal option = None with get, set
    member val unitType: int option = None with get, set
    member val limit: int = 20 with get, set
    member val skip: int = 0 with get, set
    member val orderBy: ProductOrderKeys option = None with get, set
    member val asc: bool = false with get, set
