namespace Storczyk.Prodpol.Controllers

open Microsoft.AspNetCore.Mvc

[<ApiController>]
[<Route("api/[controller]/[action]")>]
type StatusController() =
    inherit ControllerBase()
    
    member _.Ping() =
        "Pong"
    [<HttpGet>]
    member _.Health() =
        "Healthy"