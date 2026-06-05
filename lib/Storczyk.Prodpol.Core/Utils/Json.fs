module Storczyk.Prodpol.Core.Utils.Json

open System.Text.Json

let readableJson x =
    JsonSerializer.Serialize(x, JsonSerializerOptions(IndentSize = 2))
