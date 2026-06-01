namespace Storczyk.Prodpol.Utils

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc.ModelBinding
open Microsoft.FSharp.Reflection

type FSharpOptionModelBinder() =
    interface IModelBinder with
        member _.BindModelAsync(bindingContext: ModelBindingContext) =
            let modelName = bindingContext.ModelName
            let valueProviderResult = bindingContext.ValueProvider.GetValue(modelName)
            
            if valueProviderResult = ValueProviderResult.None then
                // Brak wartości w query/route -> zwraca None
                bindingContext.Result <- ModelBindingResult.Success(None)
                Task.CompletedTask
            else
                bindingContext.ModelState.SetModelValue(modelName, valueProviderResult)
                let rawValue = valueProviderResult.FirstValue
                
                if String.IsNullOrEmpty(rawValue) then
                    bindingContext.Result <- ModelBindingResult.Success(None)
                    Task.CompletedTask
                else
                    // Wyciągnięcie wewnętrznego typu z Option<'T> (np. string z string option)
                    let innerType = bindingContext.ModelType.GetGenericArguments().[0]
                    try
                        // Próba konwersji tekstu na docelowy typ wewnętrzny
                        let convertedValue = Convert.ChangeType(rawValue, innerType)
                        // Utworzenie Some(wartość)
                        let someValue = FSharpValue.MakeUnion(FSharpType.GetUnionCases(bindingContext.ModelType).[1], [| convertedValue |])
                        bindingContext.Result <- ModelBindingResult.Success(someValue)
                    with
                    | _ -> 
                        bindingContext.ModelState.TryAddModelError(modelName, "Niepoprawny format danych.") |> ignore
                        bindingContext.Result <- ModelBindingResult.Failed()
                    Task.CompletedTask

type FSharpOptionModelBinderProvider() =
    interface IModelBinderProvider with
        member _.GetBinder(context: ModelBinderProviderContext) =
            if context.Metadata.ModelType.IsGenericType &&
                context.Metadata.ModelType.GetGenericTypeDefinition().Equals(typeof<option<int>>.GetGenericTypeDefinition()) then
                FSharpOptionModelBinder() :> IModelBinder
            else
                null
