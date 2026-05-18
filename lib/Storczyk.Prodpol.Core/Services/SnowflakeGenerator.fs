namespace Storczyk.Prodpol.Core.Services

open System
open System.Threading

type ISnowflakeGenerator =
    interface
        abstract member GetSnowflake: DateTime -> int64
        abstract member GetSnowflake: unit -> int64
    end


type SnowflakeOptions = { Epoch: DateTime; InstanceId: uint64 }

module Snowflake =
    // Set epoch to 2026-05-16T17:13:07+00:00
    let DefaultSnowflakeOptions: SnowflakeOptions =
        { Epoch = DateTimeOffset.FromUnixTimeSeconds(1778882400L).DateTime
          InstanceId = 1uL }

// Snowflake structure:
// - timestamp 16 bit millisecond precision
// - instance's id * 256  |
// - counter              | 16 bits

type SnowflakeGenerator(options: SnowflakeOptions) =
    member val counter = 0u

    interface ISnowflakeGenerator with
        member this.GetSnowflake() =
            (this :> ISnowflakeGenerator).GetSnowflake(DateTime.UtcNow)

        member this.GetSnowflake(time) =
            let c = uint64 (Interlocked.Increment(ref (this.counter)))
            let t = uint64 ((time - options.Epoch).Ticks / TimeSpan.TicksPerMillisecond)
            let ci_mask = (1uL <<< 16) - 1uL

            let mutable result = 0uL

            result <- result ||| (t <<< 16)
            result <- result ||| (((c <<< 8) + (options.InstanceId)) &&& ci_mask)

            result <- result <<< 1 // clear the last bit
            result <- result >>> 1

            int64 (result)
