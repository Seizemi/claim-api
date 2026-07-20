using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Features.Integration.Tests.Infrastructure;
using Modules.Claims.Features.Integration.Tests.Shared;

namespace Modules.Claims.Features.Integration.Tests.Features.CreateClaim;

[TestClass]
public sealed class CreateClaimTests : IntegrationTestBase
{
    [TestMethod]
    public async Task CreateClaim_ValidRequest_Returns201WithLocationHeaderAndClaimId()
    {
        var request = ClaimRequestFactory.CreateValid();

        var response = await Client.PostAsJsonAsync(RouteConsts.NewClaim, request, TestJsonSerializerOptions.Default);

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Assert.IsNotNull(response.Headers.Location);

        var claimId = await response.Content.ReadFromJsonAsync<Guid>(TestJsonSerializerOptions.Default);
        Assert.AreNotEqual(Guid.Empty, claimId);

        var dbClaim = await DbContext.Claims.SingleAsync(c => c.Id == claimId);
        Assert.AreEqual(dbClaim.Reason, request.Reason);
    }

    [TestMethod]
    public async Task CreateClaim_ValidRequest_PersistsRetrievableViaGetClaimById()
    {
        var request = ClaimRequestFactory.CreateValid();

        var createResponse = await Client.PostAsJsonAsync(RouteConsts.NewClaim, request, TestJsonSerializerOptions.Default);
        var claimId = await createResponse.Content.ReadFromJsonAsync<Guid>(TestJsonSerializerOptions.Default);

        var getResponse = await Client.GetAsync(RouteConsts.ClaimDetails(claimId));
        Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);

        var claim = await getResponse.Content.ReadFromJsonAsync<ClaimResponse>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(claim);
        Assert.AreEqual(claimId, claim!.Id);
        Assert.AreEqual(request.State, claim.State);
        Assert.AreEqual(request.Solution, claim.Solution);
        Assert.AreEqual(request.Reason, claim.Reason);
        Assert.AreEqual(request.Booking.BookingNumber, claim.Booking.BookingNumber);
        Assert.AreEqual(request.Booking.SalesChannel, claim.Booking.SalesChannel);
        Assert.AreEqual(request.Booking.Customer.Name, claim.Booking.Customer.Name);
        Assert.AreEqual(request.Booking.Customer.AkioNumber, claim.Booking.Customer.AkioNumber);
        Assert.AreEqual(request.Booking.Supplier.Name, claim.Booking.Supplier.Name);
        Assert.AreEqual(request.Booking.Supplier.SupplierAkioNumber, claim.Booking.Supplier.SupplierAkioNumber);
        DateTimeOffsetAssert.AreClose(request.ClaimDate.DateOfReceivedClaim, claim.ClaimDate.DateOfReceivedClaim);
        Assert.AreEqual(request.Compensation.CustomerVoucher, claim.Compensation.CustomerVoucher);
        Assert.AreEqual(request.Compensation.RefundState, claim.Compensation.RefundState);

        var dbClaim = await DbContext.Claims.SingleAsync(c => c.Id == claimId);
        Assert.AreEqual(dbClaim.Reason, request.Reason);
    }

    [TestMethod]
    public async Task CreateClaim_EmptyBookingNumber_Returns400ValidationProblem()
    {
        var request = ClaimRequestFactory.WithEmptyBookingNumber(ClaimRequestFactory.CreateValid());

        var response = await Client.PostAsJsonAsync(RouteConsts.NewClaim, request, TestJsonSerializerOptions.Default);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.IsTrue(problem!.Errors.ContainsKey("Booking.BookingNumber"));
    }

    [TestMethod]
    public async Task CreateClaim_EmptyCustomerName_Returns400ValidationProblem()
    {
        var request = ClaimRequestFactory.WithEmptyCustomerName(ClaimRequestFactory.CreateValid());

        var response = await Client.PostAsJsonAsync(RouteConsts.NewClaim, request, TestJsonSerializerOptions.Default);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.IsTrue(problem!.Errors.ContainsKey("Booking.Customer.Name"));
    }

    [TestMethod]
    public async Task CreateClaim_DepartureAfterArrival_Returns400ValidationProblem()
    {
        var request = ClaimRequestFactory.WithDepartureAfterArrival(ClaimRequestFactory.CreateValid());

        var response = await Client.PostAsJsonAsync(RouteConsts.NewClaim, request, TestJsonSerializerOptions.Default);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.IsTrue(problem!.Errors.ContainsKey("ClaimDate"));
    }

    [TestMethod]
    public async Task CreateClaim_EmptySupplierName_Returns400ValidationProblem()
    {
        var request = ClaimRequestFactory.WithEmptySupplierName(ClaimRequestFactory.CreateValid());

        var response = await Client.PostAsJsonAsync(RouteConsts.NewClaim, request, TestJsonSerializerOptions.Default);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.IsTrue(problem!.Errors.ContainsKey("Booking.Supplier.Name"));
    }

    [TestMethod]
    public async Task CreateClaim_NullBooking_Returns400ValidationProblem()
    {
        var request = ClaimRequestFactory.WithNullBooking(ClaimRequestFactory.CreateValid());

        var response = await Client.PostAsJsonAsync(RouteConsts.NewClaim, request, TestJsonSerializerOptions.Default);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.IsTrue(problem!.Errors.ContainsKey("Booking"));
    }

    [TestMethod]
    public async Task CreateClaim_NullCustomer_Returns400ValidationProblem()
    {
        var request = ClaimRequestFactory.WithNullCustomer(ClaimRequestFactory.CreateValid());

        var response = await Client.PostAsJsonAsync(RouteConsts.NewClaim, request, TestJsonSerializerOptions.Default);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.IsTrue(problem!.Errors.ContainsKey("Booking.Customer"));
    }

    [TestMethod]
    public async Task CreateClaim_NullSupplier_Returns400ValidationProblem()
    {
        var request = ClaimRequestFactory.WithNullSupplier(ClaimRequestFactory.CreateValid());

        var response = await Client.PostAsJsonAsync(RouteConsts.NewClaim, request, TestJsonSerializerOptions.Default);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.IsTrue(problem!.Errors.ContainsKey("Booking.Supplier"));
    }

    [TestMethod]
    public async Task CreateClaim_MalformedJsonBody_Returns400BadRequest()
    {
        using var content = new StringContent("{ not valid json", Encoding.UTF8, "application/json");

        var response = await Client.PostAsync(RouteConsts.NewClaim, content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestJsonSerializerOptions.Default);
        Assert.IsNotNull(problem);
        Assert.AreEqual("Bad request", problem!.Title);
    }
}
