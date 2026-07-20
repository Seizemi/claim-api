using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.Claims.Domain.Enums;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Features.Integration.Tests.Infrastructure;
using Modules.Claims.Features.Integration.Tests.Shared;

namespace Modules.Claims.Features.Integration.Tests.Features.UpdateClaim;

[TestClass]
public sealed class UpdateClaimTests : IntegrationTestBase
{
    [TestMethod]
    public async Task UpdateClaim_ExistingClaim_Returns200AndPersistsChanges()
    {
        var original = ClaimRequestFactory.CreateValid();
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client, original);

        var newState = original.State == ClaimState.AwaitingSupplier ? ClaimState.AwaitingClient : ClaimState.AwaitingSupplier;
        var updated = original with
        {
            State = newState,
            Reason = "Updated reason",
            Compensation = original.Compensation with { CustomerVoucher = 999.5f }
        };

        var putResponse = await Client.PutAsJsonAsync(RouteConsts.ClaimDetails(claimId), updated, TestJsonSerializerOptions.Default);
        Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);

        var getResponse = await Client.GetAsync(RouteConsts.ClaimDetails(claimId));
        var claim = await getResponse.Content.ReadFromJsonAsync<ClaimResponse>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(claim);
        Assert.AreEqual(claimId, claim!.Id);
        Assert.AreEqual(newState, claim.State);
        Assert.AreEqual("Updated reason", claim.Reason);
        Assert.AreEqual(999.5f, claim.Compensation.CustomerVoucher);

        var dbClaim = await DbContext.Claims.SingleAsync(c => c.Id == claimId);
        Assert.AreEqual(newState, dbClaim.State);
    }

    [TestMethod]
    public async Task UpdateClaim_UnknownId_Returns400WithClaimCannotBeNullError()
    {
        var request = ClaimRequestFactory.CreateValid();

        var response = await Client.PutAsJsonAsync(RouteConsts.ClaimDetails(Guid.NewGuid()), request, TestJsonSerializerOptions.Default);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.IsTrue(problem!.Errors.ContainsKey("Claim.CannotBeNull"));
    }

    [TestMethod]
    public async Task UpdateClaim_InvalidNestedSupplierName_Returns400ValidationProblem()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);
        var invalidRequest = ClaimRequestFactory.WithEmptySupplierName(ClaimRequestFactory.CreateValid());

        var response = await Client.PutAsJsonAsync(RouteConsts.ClaimDetails(claimId), invalidRequest, TestJsonSerializerOptions.Default);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.IsTrue(problem!.Errors.ContainsKey("Booking.Supplier.Name"));
    }

    [TestMethod]
    public async Task UpdateClaim_EmptyGuid_Returns400WithIdCannotBeEmptyError()
    {
        var request = ClaimRequestFactory.CreateValid();

        var response = await Client.PutAsJsonAsync(RouteConsts.ClaimDetails(Guid.Empty), request, TestJsonSerializerOptions.Default);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.IsTrue(problem!.Errors.ContainsKey("ClaimId"));
    }

    [TestMethod]
    public async Task UpdateClaim_EmptyBookingNumber_Returns400ValidationProblem()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);
        var invalidRequest = ClaimRequestFactory.WithEmptyBookingNumber(ClaimRequestFactory.CreateValid());

        var response = await Client.PutAsJsonAsync(RouteConsts.ClaimDetails(claimId), invalidRequest, TestJsonSerializerOptions.Default);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.IsTrue(problem!.Errors.ContainsKey("Booking.BookingNumber"));
    }

    [TestMethod]
    public async Task UpdateClaim_EmptyCustomerName_Returns400ValidationProblem()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);
        var invalidRequest = ClaimRequestFactory.WithEmptyCustomerName(ClaimRequestFactory.CreateValid());

        var response = await Client.PutAsJsonAsync(RouteConsts.ClaimDetails(claimId), invalidRequest, TestJsonSerializerOptions.Default);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.IsTrue(problem!.Errors.ContainsKey("Booking.Customer.Name"));
    }

    [TestMethod]
    public async Task UpdateClaim_DepartureAfterArrival_Returns400ValidationProblem()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);
        var invalidRequest = ClaimRequestFactory.WithDepartureAfterArrival(ClaimRequestFactory.CreateValid());

        var response = await Client.PutAsJsonAsync(RouteConsts.ClaimDetails(claimId), invalidRequest, TestJsonSerializerOptions.Default);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.IsTrue(problem!.Errors.ContainsKey("ClaimDate"));
    }

    [TestMethod]
    public async Task UpdateClaim_MalformedJsonBody_Returns400BadRequest()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);
        using var content = new StringContent("{ not valid json", Encoding.UTF8, "application/json");

        var response = await Client.PutAsync(RouteConsts.ClaimDetails(claimId), content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.AreEqual("Bad request", problem!.Title);
    }
}
