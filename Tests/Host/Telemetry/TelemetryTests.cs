using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModularMonolith.Telemetry;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Trace;
using Xunit;

namespace ModularMonolith.Tests.Telemetry;

public sealed class TelemetryTests
{
    // A syntactically valid connection string: the tests never export anything.
    private const string ConnectionString =
        "InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://localhost/";

    [Fact]
    public void AddTelemetry_WithoutConnectionString_RegistersNothing()
    {
        var services = BuildServices(connectionString: null);

        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddTelemetry_WithConnectionString_RegistersOpenTelemetry()
    {
        var services = BuildServices(ConnectionString);

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(TracerProvider));
    }

    [Theory]
    [InlineData("/healthz", false)]
    [InlineData("/health", true)]
    [InlineData("/api/claims", true)]
    public void AddTelemetry_WithConnectionString_TracesEveryRequestButTheLivenessProbe(string path, bool traced)
    {
        using var provider = BuildServices(ConnectionString).BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<AspNetCoreTraceInstrumentationOptions>>().Get(Options.DefaultName);

        var context = new DefaultHttpContext();
        context.Request.Path = path;

        Assert.Equal(traced, options.Filter!(context));
    }

    private static ServiceCollection BuildServices(string? connectionString)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [TelemetryExtensions.ConnectionStringKey] = connectionString })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddTelemetry(configuration);

        return services;
    }
}
