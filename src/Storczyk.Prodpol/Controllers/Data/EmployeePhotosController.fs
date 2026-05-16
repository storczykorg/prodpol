namespace Storczyk.Prodpol.Controllers.Data

open Microsoft.AspNetCore.JsonPatch.SystemTextJson
open Microsoft.AspNetCore.Mvc
open Storczyk.Prodpol.Core.Models

[<ApiController>]
[<Route("api/data/employees/photos/")>]
type EmployeePhotosController() =
    inherit ControllerBase()
    
    [<HttpGet>]
    [<Route("all")>]
    member this.GetAll() =
        failwith "TODO"
        ""
    
    [<HttpGet>]
    [<Route("{id:long}")>]
    member this.GetById(id: int64) =
        failwith "TODO"
        ""
        
    [<HttpPatch>]
    [<Route("{id:long}")>]
    member this.Update([<FromBody>]update: JsonPatchDocument<Employee>) =
        failwith "TODO"
        ""
        
    [<HttpDelete>]
    [<Route("{id:long}")>]
    member this.Delete(id: int64) =
        failwith "TODO"
        ""
        
    [<HttpPost>]
    member this.Create([<FromBody>]entity: Employee) =
        failwith "TODO"
        ""