using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Modules.Claims.Domain;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Features.Integration.Tests.Infrastructure;
using Modules.Claims.Features.Integration.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Integration.Tests.Features.GetClaimsBySeason;

[Collection(IntegrationTestCollection.Name)]
public sealed class GetClaimsBySeasonTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetClaimsBySeason_ClaimsInDifferentSeasons_ReturnsOnlyMatchingSeason()
    {
        var matchingRequest = WithDateOfArrival(ClaimRequestFactory.CreateValid(), new DateOnly(2025, 8, 15));
        var otherSeasonRequest = WithDateOfArrival(ClaimRequestFactory.CreateValid(), new DateOnly(2025, 12, 25));

        var matchingId = await ClaimApiSeedHelper.SeedClaimAsync(Client, matchingRequest);
        await ClaimApiSeedHelper.SeedClaimAsync(Client, otherSeasonRequest);

        var response = await Client.GetAsync(RouteConsts.ClaimsBySeason("ete2025"), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<ClaimResponse>>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(items);
        var item = Assert.Single(items!);
        Assert.Equal(matchingId, item.Id);
    }

    [Fact]
    public async Task GetClaimsBySeason_MultipleSeededClaimsMatchingSeason_ReturnsListOrderedByReceivedDateDescending()
    {
        var oldestRequest = WithDateOfArrival(ClaimRequestFactory.CreateValid(new DateOnly(2026, 1, 1)), new DateOnly(2025, 11, 1));
        var middleRequest = WithDateOfArrival(ClaimRequestFactory.CreateValid(new DateOnly(2026, 1, 2)), new DateOnly(2025, 12, 1));
        var newestRequest = WithDateOfArrival(ClaimRequestFactory.CreateValid(new DateOnly(2026, 1, 3)), new DateOnly(2026, 1, 1));

        var oldestId = await ClaimApiSeedHelper.SeedClaimAsync(Client, oldestRequest);
        var middleId = await ClaimApiSeedHelper.SeedClaimAsync(Client, middleRequest);
        var newestId = await ClaimApiSeedHelper.SeedClaimAsync(Client, newestRequest);

        var response = await Client.GetAsync(RouteConsts.ClaimsBySeason("hiver2025-2026"), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<ClaimResponse>>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(items);
        Assert.Equal(
            new[] { newestId, middleId, oldestId },
            items!.Select(i => i.Id).ToArray());
    }

    [Fact]
    public async Task GetClaimsBySeason_NoClaimsInSeason_ReturnsEmptyList()
    {
        await ClaimApiSeedHelper.SeedClaimAsync(Client, WithDateOfArrival(ClaimRequestFactory.CreateValid(), new DateOnly(2025, 8, 15)));

        var response = await Client.GetAsync(RouteConsts.ClaimsBySeason("hiver2025-2026"), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<ClaimResponse>>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(items);
        Assert.Empty(items!);
    }

    [Fact]
    public async Task GetClaimsBySeason_InvalidSeasonSegment_Returns400ValidationProblem()
    {
        var response = await Client.GetAsync(RouteConsts.ClaimsBySeason("not-a-season"), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("SeasonValue"));
    }

    [Fact]
    public async Task GetClaimsBySeason_MatchingClaim_ReturnsFullClaimResponse()
    {
        var claimRequest = WithDateOfArrival(ClaimRequestFactory.CreateValid(), new DateOnly(2025, 8, 15));
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client, claimRequest);

        var response = await Client.GetAsync(RouteConsts.ClaimsBySeason("ete2025"), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<ClaimResponse>>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(items);
        var item = Assert.Single(items!);
        var expectedSeason = SeasonCalculator.Compute(claimRequest.ClaimDate.DateOfArrival!.Value);

        Assert.Equal(claimId, item.Id);
        Assert.Equal(claimRequest.State, item.State);
        Assert.Equal(claimRequest.FollowedById, item.FollowedBy?.Id);
        Assert.Equal(claimRequest.Booking.Customer.Name, item.Booking.Customer.Name);
        Assert.Equal(claimRequest.Booking.BookingNumber, item.Booking.BookingNumber);
        Assert.Equal(claimRequest.Booking.Supplier.Id, item.Booking.Supplier.Id);
        Assert.Equal(claimRequest.ClaimDate.DateOfReceivedClaim, item.ClaimDate.DateOfReceivedClaim);
        Assert.Equal(claimRequest.Compensation.CustomerVoucher, item.Compensation.CustomerVoucher);
        Assert.Equal(expectedSeason.SeasonLabel, item.Booking.SeasonLabel);
        Assert.Equal(expectedSeason.SeasonValue, item.Booking.SeasonValue);
    }

    private static ClaimRequest WithDateOfArrival(ClaimRequest request, DateOnly dateOfArrival) =>
        request with
        {
            ClaimDate = request.ClaimDate with
            {
                DateOfArrival = dateOfArrival,
                DateOfDeparture = dateOfArrival.AddDays(1)
            }
        };
}
