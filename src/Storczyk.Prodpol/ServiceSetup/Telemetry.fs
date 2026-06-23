namespace Storczyk.Prodpol.ServiceSetup

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Npgsql
open OpenTelemetry
open OpenTelemetry.Metrics
open OpenTelemetry.Trace

module ServiceTelemetry =
    let configure (services: IServiceCollection) =
        services
        |> _.AddOpenTelemetry()
        |> _.WithLogging(fun logging -> logging |> ignore)
        |> _.WithMetrics(fun metrics ->
            metrics.AddPrometheusExporter().AddAspNetCoreInstrumentation().AddNpgsqlInstrumentation()
            |> ignore)
        |> _.WithTracing(fun tracing -> tracing.AddAspNetCoreInstrumentation().AddNpgsql() |> ignore)
        |> _.UseOtlpExporter()
        |> ignore

        services
