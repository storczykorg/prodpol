namespace Storczyk.Prodpol.Core.Utils

open System

[<Serializable>]
[<AttributeUsage(AttributeTargets.Class ||| AttributeTargets.Struct, Inherited = true)>]
type ProdpolModelAttribute() = inherit Attribute()