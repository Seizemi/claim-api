using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Features.Integration.Tests.Infrastructure;
using Modules.Claims.Features.Integration.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Integration.Tests.Features.GetClaimById;

[Collection(IntegrationTestCollection.Name)]
public sealed class GetClaimByIdTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetClaimById_ExistingClaim_Returns200WithFullMappedResponse()
    {
        var request = ClaimRequestFactory.CreateValid();
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client, request);

        var response = await Client.GetAsync(RouteConsts.ClaimDetails(claimId), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var claim = await response.Content.ReadFromJsonAsync<ClaimResponse>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(claim);
        Assert.Equal(claimId, claim!.Id);
        Assert.Equal(request.State, claim.State);
        Assert.Equal(request.FollowedBy, claim.FollowedBy);
        Assert.Equal(request.Booking.BookingNumber, claim.Booking.BookingNumber);
        Assert.Equal(request.Booking.Customer.Name, claim.Booking.Customer.Name);
        Assert.Equal(request.Booking.Supplier.Name, claim.Booking.Supplier.Name);
        DateTimeOffsetAssert.AreClose(request.ClaimDate.DateOfReceivedClaim, claim.ClaimDate.DateOfReceivedClaim);
        Assert.Equal(request.Compensation.CustomerVoucher, claim.Compensation.CustomerVoucher);
    }

    [Fact]
    public async Task GetClaimById_UnknownId_Returns400WithClaimCannotBeNullError()
    {
        var response = await Client.GetAsync(RouteConsts.ClaimDetails(Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("Claim.CannotBeNull"));
    }

    [Fact]
    public async Task GetClaimById_EmptyGuid_Returns400WithIdCannotBeEmptyError()
    {
        var response = await Client.GetAsync(RouteConsts.ClaimDetails(Guid.Empty), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("ClaimId"));
    }

    [Fact]
    public async Task GetClaimById_MalformedGuidRouteValue_Returns400BadRequest()
    {
        var response = await Client.GetAsync("/api/v1.0/Claim/claim-details/not-a-guid/information", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.Equal("Bad request", problem!.Title);
    }
}
