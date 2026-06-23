namespace Storczyk.Prodpol.Core.Utils

open System
open System.Collections.Generic
open System.Linq
open System.Reflection
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions

[<Serializable>]
[<AttributeUsage(AttributeTargets.Class ||| AttributeTargets.Struct, Inherited = true)>]
type RegisterAsAttribute(targetType: Type, lifetime: ServiceLifetime) =
    inherit Attribute()
    member val TargetType = targetType
    member val Lifetime = lifetime

[<Serializable>]
[<AttributeUsage(AttributeTargets.Class ||| AttributeTargets.Struct, Inherited = true, AllowMultiple = true)>]
type RegisterAsTransientAttribute(targetType: Type) =
    inherit RegisterAsAttribute(targetType, ServiceLifetime.Transient)

[<Serializable>]
[<AttributeUsage(AttributeTargets.Class ||| AttributeTargets.Struct, Inherited = true)>]
type RegisterAsTransientAttribute<'T>() =
    inherit RegisterAsTransientAttribute(typeof<'T>)


module RegisterServiceExtensions =
    let GroupMany (x: IEnumerable<'T>) (selector: 'T -> IEnumerable<'a>) =
        x.SelectMany(fun a -> selector(a).Select(fun y -> struct (a, y)))

    type IServiceCollection with
        member this.RegisterFromRuntime() =
            for assembly in System.AppDomain.CurrentDomain.GetAssemblies() do
                for (service: Type, _attr: Attribute) in GroupMany (assembly.GetTypes()) (_.GetCustomAttributes()) do
                    match _attr with
                    | :? RegisterAsAttribute as attr ->
                        let descriptor = ServiceDescriptor(attr.TargetType, service, attr.Lifetime)
                        this.TryAdd(descriptor) |> ignore
                        printfn $"Registered using {descriptor}"

                    | _ -> ()

            this
