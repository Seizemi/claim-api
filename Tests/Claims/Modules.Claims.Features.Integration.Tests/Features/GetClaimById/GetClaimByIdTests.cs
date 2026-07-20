using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Features.Integration.Tests.Infrastructure;
using Modules.Claims.Features.Integration.Tests.Shared;

namespace Modules.Claims.Features.Integration.Tests.Features.GetClaimById;

[TestClass]
public sealed class GetClaimByIdTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetClaimById_ExistingClaim_Returns200WithFullMappedResponse()
    {
        var request = ClaimRequestFactory.CreateValid();
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client, request);

        var response = await Client.GetAsync(RouteConsts.ClaimDetails(claimId));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var claim = await response.Content.ReadFromJsonAsync<ClaimResponse>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(claim);
        Assert.AreEqual(claimId, claim!.Id);
        Assert.AreEqual(request.State, claim.State);
        Assert.AreEqual(request.FollowedBy, claim.FollowedBy);
        Assert.AreEqual(request.Booking.BookingNumber, claim.Booking.BookingNumber);
        Assert.AreEqual(request.Booking.Customer.Name, claim.Booking.Customer.Name);
        Assert.AreEqual(request.Booking.Supplier.Name, claim.Booking.Supplier.Name);
        DateTimeOffsetAssert.AreClose(request.ClaimDate.DateOfReceivedClaim, claim.ClaimDate.DateOfReceivedClaim);
        Assert.AreEqual(request.Compensation.CustomerVoucher, claim.Compensation.CustomerVoucher);
    }

    [TestMethod]
    public async Task GetClaimById_UnknownId_Returns400WithClaimCannotBeNullError()
    {
        var response = await Client.GetAsync(RouteConsts.ClaimDetails(Guid.NewGuid()));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.IsTrue(problem!.Errors.ContainsKey("Claim.CannotBeNull"));
    }

    [TestMethod]
    public async Task GetClaimById_EmptyGuid_Returns400WithIdCannotBeEmptyError()
    {
        var response = await Client.GetAsync(RouteConsts.ClaimDetails(Guid.Empty));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.IsTrue(problem!.Errors.ContainsKey("ClaimId"));
    }

    [TestMethod]
    public async Task GetClaimById_MalformedGuidRouteValue_Returns400BadRequest()
    {
        var response = await Client.GetAsync("/api/v1.0/Claim/claim-details/not-a-guid/information");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.AreEqual("Bad request", problem!.Title);
    }
}
