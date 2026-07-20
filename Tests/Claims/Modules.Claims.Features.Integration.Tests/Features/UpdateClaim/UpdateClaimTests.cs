using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.Claims.Domain.Enums;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Features.Integration.Tests.Infrastructure;
using Modules.Claims.Features.Integration.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Integration.Tests.Features.UpdateClaim;

[Collection(IntegrationTestCollection.Name)]
public sealed class UpdateClaimTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
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

        var putResponse = await Client.PutAsJsonAsync(RouteConsts.ClaimDetails(claimId), updated, TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var getResponse = await Client.GetAsync(RouteConsts.ClaimDetails(claimId), TestContext.Current.CancellationToken);
        var claim = await getResponse.Content.ReadFromJsonAsync<ClaimResponse>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(claim);
        Assert.Equal(claimId, claim!.Id);
        Assert.Equal(newState, claim.State);
        Assert.Equal("Updated reason", claim.Reason);
        Assert.Equal(999.5f, claim.Compensation.CustomerVoucher);

        var dbClaim = await DbContext.Claims.SingleAsync(c => c.Id == claimId, TestContext.Current.CancellationToken);
        Assert.Equal(newState, dbClaim.State);
    }

    [Fact]
    public async Task UpdateClaim_UnknownId_Returns400WithClaimCannotBeNullError()
    {
        var request = ClaimRequestFactory.CreateValid();

        var response = await Client.PutAsJsonAsync(RouteConsts.ClaimDetails(Guid.NewGuid()), request, TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("Claim.CannotBeNull"));
    }

    [Fact]
    public async Task UpdateClaim_InvalidNestedSupplierName_Returns400ValidationProblem()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);
        var invalidRequest = ClaimRequestFactory.WithEmptySupplierName(ClaimRequestFactory.CreateValid());

        var response = await Client.PutAsJsonAsync(RouteConsts.ClaimDetails(claimId), invalidRequest, TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("Booking.Supplier.Name"));
    }

    [Fact]
    public async Task UpdateClaim_EmptyGuid_Returns400WithIdCannotBeEmptyError()
    {
        var request = ClaimRequestFactory.CreateValid();

        var response = await Client.PutAsJsonAsync(RouteConsts.ClaimDetails(Guid.Empty), request, TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("ClaimId"));
    }

    [Fact]
    public async Task UpdateClaim_EmptyBookingNumber_Returns400ValidationProblem()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);
        var invalidRequest = ClaimRequestFactory.WithEmptyBookingNumber(ClaimRequestFactory.CreateValid());

        var response = await Client.PutAsJsonAsync(RouteConsts.ClaimDetails(claimId), invalidRequest, TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("Booking.BookingNumber"));
    }

    [Fact]
    public async Task UpdateClaim_EmptyCustomerName_Returns400ValidationProblem()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);
        var invalidRequest = ClaimRequestFactory.WithEmptyCustomerName(ClaimRequestFactory.CreateValid());

        var response = await Client.PutAsJsonAsync(RouteConsts.ClaimDetails(claimId), invalidRequest, TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("Booking.Customer.Name"));
    }

    [Fact]
    public async Task UpdateClaim_DepartureAfterArrival_Returns400ValidationProblem()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);
        var invalidRequest = ClaimRequestFactory.WithDepartureAfterArrival(ClaimRequestFactory.CreateValid());

        var response = await Client.PutAsJsonAsync(RouteConsts.ClaimDetails(claimId), invalidRequest, TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("ClaimDate"));
    }

    [Fact]
    public async Task UpdateClaim_MalformedJsonBody_Returns400BadRequest()
    {
        var claimId = await ClaimApiSeedHelper.SeedClaimAsync(Client);
        using var content = new StringContent("{ not valid json", Encoding.UTF8, "application/json");

        var response = await Client.PutAsync(RouteConsts.ClaimDetails(claimId), content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.Equal("Bad request", problem!.Title);
    }
}
