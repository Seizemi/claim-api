using AutoFixture;
using Modules.Claims.Domain.Enums;
using Modules.Claims.Features.Features.Shared.Requests;

namespace Modules.Claims.Features.Integration.Tests.Shared;

internal static class ClaimRequestFactory
{
    internal static ClaimRequest CreateValid(DateOnly? dateOfReceivedClaim = null, ClaimState? state = null)
    {
        var fixture = new Fixture();

        var dateOfArrival = DateOnly.FromDateTime(fixture.Create<DateTime>());
        var dateOfDeparture = dateOfArrival.AddDays(Math.Abs(fixture.Create<int>()) % 30);

        return new ClaimRequest(
            State: state ?? fixture.Create<ClaimState>(),
            FollowedById: LookupTestIds.FollowedById,
            ReasonId: LookupTestIds.ReasonId,
            ClaimSummary: fixture.Create<string>(),
            SolutionId: LookupTestIds.SolutionId,
            PurposeOfSolution: fixture.Create<string>(),
            UpdateReason: null,
            CustomerSuppInfo: fixture.Create<string>(),
            SupplierSuppInfo: fixture.Create<string>(),
            Booking: new BookingRequest(
                BookingNumber: fixture.Create<string>(),
                SalesChannelId: LookupTestIds.SalesChannelId,
                SkissimTypeId: LookupTestIds.SkissimTypeId,
                Product: fixture.Create<string>(),
                Customer: new CustomerRequest(
                    Name: fixture.Create<string>(),
                    AkioNumber: fixture.Create<int>()),
                Supplier: new SupplierRequest(
                    Id: LookupTestIds.SupplierId)),
            ClaimDate: new ClaimDateRequest(
                DateOfReceivedClaim: dateOfReceivedClaim ?? DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-fixture.Create<int>() % 30),
                DateOfStartFollowUp: null,
                DateLastUpdate: null,
                DateOfDeparture: dateOfDeparture,
                DateEndOfFollowUp: null,
                DateOfArrival: dateOfArrival),
            Compensation: new CompensationRequest(
                CustomerVoucher: fixture.Create<float>(),
                CustomerUsedVoucher: null,
                SupplierRefund: null,
                ClaimRefund: null,
                RefundStateId: LookupTestIds.RefundStateId,
                CompensationReasonId: LookupTestIds.CompensationReasonId));
    }

    internal static ClaimRequest WithEmptyBookingNumber(ClaimRequest request) =>
        request with { Booking = request.Booking with { BookingNumber = string.Empty } };

    internal static ClaimRequest WithEmptyCustomerName(ClaimRequest request) =>
        request with
        {
            Booking = request.Booking with
            {
                Customer = request.Booking.Customer with { Name = string.Empty }
            }
        };

    internal static ClaimRequest WithEmptySupplierId(ClaimRequest request) =>
        request with
        {
            Booking = request.Booking with
            {
                Supplier = request.Booking.Supplier with { Id = Guid.Empty }
            }
        };

    internal static ClaimRequest WithNullBooking(ClaimRequest request) =>
        request with { Booking = null! };

    internal static ClaimRequest WithNullCustomer(ClaimRequest request) =>
        request with { Booking = request.Booking with { Customer = null! } };

    internal static ClaimRequest WithNullSupplier(ClaimRequest request) =>
        request with { Booking = request.Booking with { Supplier = null! } };

    internal static ClaimRequest WithDepartureBeforeArrival(ClaimRequest request) =>
        request with
        {
            ClaimDate = request.ClaimDate with
            {
                DateOfDeparture = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1),
                DateOfArrival = DateOnly.FromDateTime(DateTime.UtcNow)
            }
        };
}
