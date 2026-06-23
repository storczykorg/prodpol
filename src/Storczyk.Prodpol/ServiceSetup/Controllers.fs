namespace Storczyk.Prodpol.ServiceSetup

open System.Reflection
open System.Text.Json.Serialization
open Microsoft.Extensions.DependencyInjection
open Storczyk.Prodpol.Utils

module ServiceControllers =
    let configure (services: IServiceCollection) =
        let mainAs = Assembly.GetExecutingAssembly()

        services
            .AddControllers()
            .AddMvcOptions(fun options ->
                options.ModelBinderProviders.Insert(0, FSharpOptionModelBinderProvider())
                ())
            .AddJsonOptions(fun options ->
                let fsharpConverter = JsonFSharpConverter(JsonFSharpOptions.FSharpLuLike())
                options.JsonSerializerOptions.Converters.Insert(0, fsharpConverter))
            .AddApplicationPart(mainAs)
        |> ignore

        services
