using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Modules.Claims.Domain.Enums;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Features.Integration.Tests.Infrastructure;
using Modules.Claims.Features.Integration.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Integration.Tests.Features.GetClaimsByState;

[Collection(IntegrationTestCollection.Name)]
public sealed class GetClaimsByStateTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetClaimsByState_ClaimsInMultipleStates_ReturnsOnlyMatchingState()
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var matchingId = await ClaimApiSeedHelper.SeedClaimAsync(
            Client,
            ClaimRequestFactory.CreateValid(baseDate, ClaimState.AwaitingSupplier));
        await ClaimApiSeedHelper.SeedClaimAsync(
            Client,
            ClaimRequestFactory.CreateValid(baseDate, ClaimState.AwaitingClient));

        var response = await Client.GetAsync(RouteConsts.ClaimsByState(ClaimState.AwaitingSupplier), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paged = await response.Content.ReadFromJsonAsync<PagedResponse>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(paged);
        Assert.Equal(1, paged!.TotalCount);
        var item = Assert.Single(paged.Items);
        Assert.Equal(matchingId, item.Id);
    }

    [Fact]
    public async Task GetClaimsByState_MultipleSeededClaimsMatchingState_ReturnsPagedResponseOrderedByReceivedDateDescending()
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var oldestId = await ClaimApiSeedHelper.SeedClaimAsync(Client, ClaimRequestFactory.CreateValid(baseDate, ClaimState.Terminate));
        var middleId = await ClaimApiSeedHelper.SeedClaimAsync(Client, ClaimRequestFactory.CreateValid(baseDate.AddDays(1), ClaimState.Terminate));
        var newestId = await ClaimApiSeedHelper.SeedClaimAsync(Client, ClaimRequestFactory.CreateValid(baseDate.AddDays(2), ClaimState.Terminate));

        var response = await Client.GetAsync(RouteConsts.ClaimsByState(ClaimState.Terminate), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paged = await response.Content.ReadFromJsonAsync<PagedResponse>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(paged);
        Assert.Equal(3, paged!.TotalCount);
        Assert.Equal(1, paged.TotalPages);
        Assert.Equal(
            new[] { newestId, middleId, oldestId },
            paged.Items.Select(i => i.Id).ToArray());
    }

    [Fact]
    public async Task GetClaimsByState_PageSizeSmallerThanTotal_ReturnsCorrectPageAndTotalPages()
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var ids = new List<Guid>();
        for (var i = 0; i < 5; i++)
        {
            ids.Add(await ClaimApiSeedHelper.SeedClaimAsync(Client, ClaimRequestFactory.CreateValid(baseDate.AddDays(i), ClaimState.ClosedWithoutResponse)));
        }

        // Newest first: ids[4], ids[3], ids[2], ids[1], ids[0]. Page 2 of size 2 -> ids[2], ids[1].
        var response = await Client.GetAsync(
            $"{RouteConsts.ClaimsByState(ClaimState.ClosedWithoutResponse)}?PageNumber=2&PageSize=2",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paged = await response.Content.ReadFromJsonAsync<PagedResponse>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(paged);
        Assert.Equal(5, paged!.TotalCount);
        Assert.Equal(3, paged.TotalPages);
        Assert.Equal(2, paged.PageNumber);
        Assert.Equal(2, paged.PageSize);
        Assert.Equal(
            new[] { ids[2], ids[1] },
            paged.Items.Select(i => i.Id).ToArray());
    }

    [Fact]
    public async Task GetClaimsByState_NoClaimsInState_ReturnsEmptyPagedResponse()
    {
        await ClaimApiSeedHelper.SeedClaimAsync(Client, ClaimRequestFactory.CreateValid(state: ClaimState.AwaitingClient));

        var response = await Client.GetAsync(RouteConsts.ClaimsByState(ClaimState.AwaitingSupplier), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paged = await response.Content.ReadFromJsonAsync<PagedResponse>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(paged);
        Assert.Equal(0, paged!.TotalCount);
        Assert.Equal(0, paged.TotalPages);
        Assert.Empty(paged.Items);
    }

    [Fact]
    public async Task GetClaimsByState_InvalidStateSegment_Returns400BadRequest()
    {
        var response = await Client.GetAsync("/api/v1.0/Claim/by-state/NotAState", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.Equal("Bad request", problem!.Title);
    }

    [Fact]
    public async Task GetClaimsByState_PageSizeZero_Returns400ValidationProblem()
    {
        var response = await Client.GetAsync(
            $"{RouteConsts.ClaimsByState(ClaimState.AwaitingSupplier)}?PageSize=0",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("PageSize"));
    }

    [Fact]
    public async Task GetClaimsByState_PageNumberZero_Returns400ValidationProblem()
    {
        var response = await Client.GetAsync(
            $"{RouteConsts.ClaimsByState(ClaimState.AwaitingSupplier)}?PageNumber=0",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("PageNumber"));
    }
}
