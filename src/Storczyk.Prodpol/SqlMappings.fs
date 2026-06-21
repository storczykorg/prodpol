module Storczyk.Prodpol.SqlMappings

open System
open System.ComponentModel.DataAnnotations.Schema
open System.Linq
open System.Reflection
open Dapper
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Utils

    let mapProperty(_type: Type) (name: string) : PropertyInfo =
        _type.GetProperties().FirstOrDefault(
            fun prop ->
            (
                prop.GetCustomAttributes<ColumnAttribute>(true)
                     .Any(fun attr -> attr.Name = name))
            || (prop.GetCustomAttributes<LinqToDB.Mapping.ColumnAttribute>(true).Any(fun attr -> attr.Name = name))
            )
    let addTypeMapping<'T>() =
        SqlMapper.SetTypeMap(
            typedefof<'T>,
            CustomPropertyTypeMap(
                typedefof<'T>,
                mapProperty
                )
            )
        printfn $"Mapped {typedefof<'T>}"
    let addTypedefMapping(_type: Type) =
        SqlMapper.SetTypeMap(
            _type,
            CustomPropertyTypeMap(
                _type,
                mapProperty
                )
            )
    let applyMappings() =

        addTypeMapping<EmployeeRead>()
        addTypeMapping<EmployeeRoleRead>()
        addTypeMapping<EmployeeRole>()
        addTypeMapping<EmployeePhoto>()
        addTypeMapping<Employee>()
        for i in (AppDomain.CurrentDomain.GetAssemblies()
                      .SelectMany(fun x -> x.GetTypes().AsEnumerable())
                      .Where(fun x -> ProdpolModelAttribute.GetCustomAttributes(x).Any())) do
            addTypedefMapping(i.GetTypeInfo())
            ()

