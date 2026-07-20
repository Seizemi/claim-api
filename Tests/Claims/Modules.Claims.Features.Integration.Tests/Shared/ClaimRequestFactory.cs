using AutoFixture;
using Modules.Claims.Domain.Enums;
using Modules.Claims.Features.Features.Shared.Requests;

namespace Modules.Claims.Features.Integration.Tests.Shared;

internal static class ClaimRequestFactory
{
    internal static ClaimRequest CreateValid(DateTimeOffset? dateOfReceivedClaim = null)
    {
        var fixture = new Fixture();

        return new ClaimRequest(
            State: fixture.Create<ClaimState>(),
            FollowedBy: fixture.Create<string>(),
            Reason: fixture.Create<string>(),
            ClaimSummary: fixture.Create<string>(),
            Solution: fixture.Create<ClaimSolution>(),
            PurposeOfSolution: fixture.Create<string>(),
            UpdateReason: null,
            CustomerSuppInfo: fixture.Create<string>(),
            SupplierSuppInfo: fixture.Create<string>(),
            Booking: new BookingRequest(
                BookingNumber: fixture.Create<string>(),
                SalesChannel: fixture.Create<SalesChannel>(),
                Language: fixture.Create<Language>(),
                SeasonLabel: fixture.Create<string>(),
                SeasonValue: fixture.Create<string>(),
                Service: fixture.Create<BookingService>(),
                Skissim: fixture.Create<bool>(),
                SkissimType: fixture.Create<SkissimType>(),
                Product: fixture.Create<string>(),
                Customer: new CustomerRequest(
                    Name: fixture.Create<string>(),
                    AkioNumber: fixture.Create<int>()),
                Supplier: new SupplierRequest(
                    Name: fixture.Create<string>(),
                    SupplierAkioNumber: fixture.Create<int>())),
            // DateOfDeparture/DateOfArrival are left null: ClaimRequestValidator requires
            // DateOfDeparture <= DateOfArrival whenever both are set, which two independently
            // generated values can't guarantee.
            ClaimDate: new ClaimDateRequest(
                DateOfReceivedClaim: dateOfReceivedClaim ?? DateTimeOffset.UtcNow.AddDays(-fixture.Create<int>() % 30),
                DateOfStartFollowUp: null,
                DateLastUpdate: null,
                DateOfDeparture: null,
                DateEndOfFollowUp: null,
                DateOfArrival: null),
            Compensation: new CompensationRequest(
                CustomerVoucher: fixture.Create<float>(),
                CustomerUsedVoucher: null,
                SupplierRefund: null,
                ClaimRefund: null,
                RefundState: fixture.Create<RefundState>()));
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

    internal static ClaimRequest WithEmptySupplierName(ClaimRequest request) =>
        request with
        {
            Booking = request.Booking with
            {
                Supplier = request.Booking.Supplier with { Name = string.Empty }
            }
        };

    internal static ClaimRequest WithNullBooking(ClaimRequest request) =>
        request with { Booking = null! };

    internal static ClaimRequest WithNullCustomer(ClaimRequest request) =>
        request with { Booking = request.Booking with { Customer = null! } };

    internal static ClaimRequest WithNullSupplier(ClaimRequest request) =>
        request with { Booking = request.Booking with { Supplier = null! } };

    internal static ClaimRequest WithDepartureAfterArrival(ClaimRequest request) =>
        request with
        {
            ClaimDate = request.ClaimDate with
            {
                DateOfDeparture = DateTimeOffset.UtcNow,
                DateOfArrival = DateTimeOffset.UtcNow.AddDays(-1)
            }
        };
}
