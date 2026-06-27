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
                    let innerType = bindingContext.ModelType.GetGenericArguments().[0]

                    try
                        let convertedValue =
                            if innerType.IsArray then
                                let elementType = innerType.GetElementType()
                                let allValues =
                                    valueProviderResult.Values
                                    |> Seq.collect (fun v ->
                                        if isNull v then Seq.empty
                                        else v.Split(',') |> Seq.map (fun s -> s.Trim()))
                                    |> Seq.filter (not << String.IsNullOrEmpty)
                                    |> Seq.toArray
                                let typedArray = Array.CreateInstance(elementType, allValues.Length)
                                allValues
                                |> Array.iteri (fun i v ->
                                    typedArray.SetValue(Convert.ChangeType(v, elementType), i))
                                box typedArray
                            else
                                Convert.ChangeType(rawValue, innerType)

                        let someValue =
                            FSharpValue.MakeUnion(
                                FSharpType.GetUnionCases(bindingContext.ModelType).[1],
                                [| convertedValue |]
                            )

                        bindingContext.Result <- ModelBindingResult.Success(someValue)
                    with _ ->
                        bindingContext.ModelState.TryAddModelError(modelName, "Niepoprawny format danych.")
                        |> ignore

                        bindingContext.Result <- ModelBindingResult.Failed()

                    Task.CompletedTask

type FSharpOptionModelBinderProvider() =
    interface IModelBinderProvider with
        member _.GetBinder(context: ModelBinderProviderContext) =
            if
                context.Metadata.ModelType.IsGenericType
                && context.Metadata.ModelType.GetGenericTypeDefinition() = typedefof<_ option>
            then
                FSharpOptionModelBinder() :> IModelBinder
            else
                null
