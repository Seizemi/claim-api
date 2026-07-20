using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Features.Integration.Tests.Infrastructure;
using Modules.Claims.Features.Integration.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Integration.Tests.Features.CreateClaim;

[Collection(IntegrationTestCollection.Name)]
public sealed class CreateClaimTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateClaim_ValidRequest_Returns201WithLocationHeaderAndClaimId()
    {
        var request = ClaimRequestFactory.CreateValid();

        var response = await Client.PostAsJsonAsync(RouteConsts.NewClaim, request, TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var claimId = await response.Content.ReadFromJsonAsync<Guid>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotEqual(Guid.Empty, claimId);

        var dbClaim = await DbContext.Claims.SingleAsync(c => c.Id == claimId, TestContext.Current.CancellationToken);
        Assert.Equal(dbClaim.Reason, request.Reason);
    }

    [Fact]
    public async Task CreateClaim_ValidRequest_PersistsRetrievableViaGetClaimById()
    {
        var request = ClaimRequestFactory.CreateValid();

        var createResponse = await Client.PostAsJsonAsync(RouteConsts.NewClaim, request, TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        var claimId = await createResponse.Content.ReadFromJsonAsync<Guid>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);

        var getResponse = await Client.GetAsync(RouteConsts.ClaimDetails(claimId), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var claim = await getResponse.Content.ReadFromJsonAsync<ClaimResponse>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(claim);
        Assert.Equal(claimId, claim!.Id);
        Assert.Equal(request.State, claim.State);
        Assert.Equal(request.Solution, claim.Solution);
        Assert.Equal(request.Reason, claim.Reason);
        Assert.Equal(request.Booking.BookingNumber, claim.Booking.BookingNumber);
        Assert.Equal(request.Booking.SalesChannel, claim.Booking.SalesChannel);
        Assert.Equal(request.Booking.Customer.Name, claim.Booking.Customer.Name);
        Assert.Equal(request.Booking.Customer.AkioNumber, claim.Booking.Customer.AkioNumber);
        Assert.Equal(request.Booking.Supplier.Name, claim.Booking.Supplier.Name);
        Assert.Equal(request.Booking.Supplier.SupplierAkioNumber, claim.Booking.Supplier.SupplierAkioNumber);
        DateTimeOffsetAssert.AreClose(request.ClaimDate.DateOfReceivedClaim, claim.ClaimDate.DateOfReceivedClaim);
        Assert.Equal(request.Compensation.CustomerVoucher, claim.Compensation.CustomerVoucher);
        Assert.Equal(request.Compensation.RefundState, claim.Compensation.RefundState);

        var dbClaim = await DbContext.Claims.SingleAsync(c => c.Id == claimId, TestContext.Current.CancellationToken);
        Assert.Equal(dbClaim.Reason, request.Reason);
    }

    [Fact]
    public async Task CreateClaim_EmptyBookingNumber_Returns400ValidationProblem()
    {
        var request = ClaimRequestFactory.WithEmptyBookingNumber(ClaimRequestFactory.CreateValid());

        var response = await Client.PostAsJsonAsync(RouteConsts.NewClaim, request, TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("Booking.BookingNumber"));
    }

    [Fact]
    public async Task CreateClaim_EmptyCustomerName_Returns400ValidationProblem()
    {
        var request = ClaimRequestFactory.WithEmptyCustomerName(ClaimRequestFactory.CreateValid());

        var response = await Client.PostAsJsonAsync(RouteConsts.NewClaim, request, TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("Booking.Customer.Name"));
    }

    [Fact]
    public async Task CreateClaim_DepartureBeforeArrival_Returns400ValidationProblem()
    {
        var request = ClaimRequestFactory.WithDepartureBeforeArrival(ClaimRequestFactory.CreateValid());

        var response = await Client.PostAsJsonAsync(RouteConsts.NewClaim, request, TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("ClaimDate"));
    }

    [Fact]
    public async Task CreateClaim_EmptySupplierName_Returns400ValidationProblem()
    {
        var request = ClaimRequestFactory.WithEmptySupplierName(ClaimRequestFactory.CreateValid());

        var response = await Client.PostAsJsonAsync(RouteConsts.NewClaim, request, TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("Booking.Supplier.Name"));
    }

    [Fact]
    public async Task CreateClaim_NullBooking_Returns400ValidationProblem()
    {
        var request = ClaimRequestFactory.WithNullBooking(ClaimRequestFactory.CreateValid());

        var response = await Client.PostAsJsonAsync(RouteConsts.NewClaim, request, TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("Booking"));
    }

    [Fact]
    public async Task CreateClaim_NullCustomer_Returns400ValidationProblem()
    {
        var request = ClaimRequestFactory.WithNullCustomer(ClaimRequestFactory.CreateValid());

        var response = await Client.PostAsJsonAsync(RouteConsts.NewClaim, request, TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("Booking.Customer"));
    }

    [Fact]
    public async Task CreateClaim_NullSupplier_Returns400ValidationProblem()
    {
        var request = ClaimRequestFactory.WithNullSupplier(ClaimRequestFactory.CreateValid());

        var response = await Client.PostAsJsonAsync(RouteConsts.NewClaim, request, TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("Booking.Supplier"));
    }

    [Fact]
    public async Task CreateClaim_MalformedJsonBody_Returns400BadRequest()
    {
        using var content = new StringContent("{ not valid json", Encoding.UTF8, "application/json");

        var response = await Client.PostAsync(RouteConsts.NewClaim, content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestJsonSerializerOptions.Default, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.Equal("Bad request", problem!.Title);
    }
}
