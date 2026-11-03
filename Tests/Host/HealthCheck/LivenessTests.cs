using System.Net;
using ModularMonolith.HealthCheck;
using ModularMonolith.Testing;
using Xunit;

namespace ModularMonolith.Tests.HealthCheck;

public sealed class LivenessTests
{
    [Fact]
    public async Task Healthz_WithVersion_Returns200WithVersionHeader()
    {
        var settings = new Dictionary<string, string?>(BffTestHost.ValidSettings)
        {
            [HealthCheckEndpoint.VersionSetting] = "abc123-def456"
        };
        await using var host = await BffTestHost.StartAsync(settings);

        var response = await host.SendAsync(HttpMethod.Get, "/healthz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("abc123-def456", Assert.Single(response.Headers.GetValues(HealthCheckEndpoint.VersionHeader)));
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Healthz_WithoutVersion_Returns200WithoutVersionHeader(string? version)
    {
        var settings = new Dictionary<string, string?>(BffTestHost.ValidSettings)
        {
            [HealthCheckEndpoint.VersionSetting] = version
        };
        await using var host = await BffTestHost.StartAsync(settings);

        var response = await host.SendAsync(HttpMethod.Get, "/healthz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(response.Headers.Contains(HealthCheckEndpoint.VersionHeader));
    }
}
