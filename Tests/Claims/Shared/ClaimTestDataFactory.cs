using AutoFixture;
using Modules.Claims.Domain.Entities;
using Modules.Claims.Domain.Enums;
using Modules.Claims.Features.Features.Shared.Requests;

namespace Modules.Claims.Features.Tests.Shared;

internal static class ClaimTestDataFactory
{
    internal static Claim CreateClaim(DateTimeOffset dateOfReceivedClaim, ClaimState? state = null)
    {
        var fixture = new Fixture();
        var claimId = Guid.CreateVersion7();
        var bookingId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var supplierId = Guid.CreateVersion7();

        return new Claim
        {
            Id = claimId,
            State = state ?? fixture.Create<ClaimState>(),
            BookingId = bookingId,
            Booking = new Booking
            {
                Id = bookingId,
                BookingNumber = fixture.Create<string>(),
                SeasonLabel = fixture.Create<string>(),
                SeasonValue = fixture.Create<string>(),
                CustomerId = customerId,
                Customer = new Customer
                {
                    Id = customerId,
                    Name = fixture.Create<string>(),
                    AkioNumber = fixture.Create<int>()
                },
                SupplierId = supplierId,
                Supplier = new Supplier
                {
                    Id = supplierId,
                    Name = fixture.Create<string>(),
                    SupplierAkioNumber = fixture.Create<int>()
                }
            },
            ClaimDate = new ClaimDate
            {
                Id = Guid.CreateVersion7(),
                ClaimId = claimId,
                DateOfReceivedClaim = dateOfReceivedClaim
            },
            Compensation = new Compensation
            {
                Id = Guid.CreateVersion7(),
                ClaimId = claimId
            }
        };
    }

    internal static ClaimRequest CreateClaimRequest()
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
                DateOfReceivedClaim: fixture.Create<DateTimeOffset>(),
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
}
