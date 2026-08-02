using AutoFixture;
using Modules.Claims.Domain.Enums;
using Modules.Claims.Features.Features.Shared.Requests;

namespace Modules.Claims.Features.Load.Tests.Shared;

internal static class ClaimRequestFactory
{
    internal static ClaimRequest CreateValid()
    {
        var fixture = new Fixture();

        var dateOfArrival = fixture.Create<DateTimeOffset>().ToUniversalTime();
        var dateOfDeparture = dateOfArrival.AddDays(Math.Abs(fixture.Create<int>()) % 30);

        return new ClaimRequest(
            State: fixture.Create<ClaimState>(),
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
                DateOfReceivedClaim: DateTimeOffset.UtcNow.AddDays(-fixture.Create<int>() % 30),
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
}
