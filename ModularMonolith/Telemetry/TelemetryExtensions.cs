using Azure.Monitor.OpenTelemetry.AspNetCore;
using ModularMonolith.Routes;
using Npgsql;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ModularMonolith.Telemetry;

internal static class TelemetryExtensions
{
    internal const string ConnectionStringKey = "APPLICATIONINSIGHTS_CONNECTION_STRING";

    // The cloud role name in Application Insights.
    internal const string ServiceName = "ClaimApi";

    /// <summary>
    /// Requests, outgoing HTTP calls (Microsoft Graph), PostgreSQL commands, metrics and ILogger logs, exported to
    /// Application Insights. Only when its connection string is set (Azure): nothing is collected locally.
    /// </summary>
    internal static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration[ConnectionStringKey]))
        {
            return services;
        }

        // The distro reads the connection string from the same setting.
        services.AddOpenTelemetry()
            .UseAzureMonitor()
            .ConfigureResource(resource => resource.AddService(ServiceName))
            .WithTracing(tracing => tracing.AddNpgsql())
            .WithMetrics(metrics => metrics.AddMeter("Npgsql"));

        // App Service calls the liveness probe every minute: its requests would only add noise and cost.
        services.Configure<AspNetCoreTraceInstrumentationOptions>(options =>
            options.Filter = context => !context.Request.Path.StartsWithSegments(RouteConsts.Liveness));

        return services;
    }
}
