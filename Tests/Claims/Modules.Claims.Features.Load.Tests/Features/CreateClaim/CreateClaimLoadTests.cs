using System.Net.Http.Json;
using Modules.Claims.Features.Load.Tests.Infrastructure;
using Modules.Claims.Features.Load.Tests.Shared;
using NBomber.CSharp;
using Xunit;

namespace Modules.Claims.Features.Load.Tests.Features.CreateClaim;

[Collection(LoadTestCollection.Name)]
public sealed class CreateClaimLoadTests(LoadTestWebAppFactory factory)
{
    [Fact]
    public void CreateClaim_UnderSustainedLoad_MeetsSuccessAndLatencyThresholds()
    {
        var client = factory.CreateClient();
        var samples = Enumerable.Range(0, 500)
            .Select(_ => ClaimRequestFactory.CreateValid())
            .ToArray();

        var scenario = Scenario.Create("create_claim", async context =>
        {
            var request = samples[Random.Shared.Next(samples.Length)];
            var response = await client.PostAsJsonAsync(RouteConsts.NewClaim, request, TestJsonSerializerOptions.Default);

            return response.IsSuccessStatusCode
                ? Response.Ok(statusCode: response.StatusCode.ToString())
                : Response.Fail(statusCode: response.StatusCode.ToString());
        })
        .WithLoadSimulations(
            Simulation.RampingConstant(copies: 20, TimeSpan.FromSeconds(15)),
            Simulation.KeepConstant(copies: 20, TimeSpan.FromSeconds(30)));

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("load-test-reports")
            .Run();

        var scenarioStats = stats.ScenarioStats[0];
        Assert.True(scenarioStats.Fail.Request.Count == 0,
            $"Expected no failed requests, got {scenarioStats.Fail.Request.Count}");
        Assert.True(scenarioStats.Ok.Latency.Percent95 < 180,
            $"P95 latency was {scenarioStats.Ok.Latency.Percent95}ms, expected under 180ms");
    }
}
