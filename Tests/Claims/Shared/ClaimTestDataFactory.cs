using AutoFixture;
using Modules.Claims.Domain.Entities;
using Modules.Claims.Domain.Enums;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Infrastructure.Database;

namespace Modules.Claims.Features.Tests.Shared;

internal static class ClaimTestDataFactory
{
    internal static async Task SeedLookupsAsync(ClaimsDbContext context, ClaimRequest request, CancellationToken cancellationToken = default)
    {
        context.Reasons.Add(new Reason { Id = request.ReasonId, Label = "Reason", Value = "Reason" });
        context.Solutions.Add(new Solution { Id = request.SolutionId, Label = "Solution", Value = "Solution" });
        if (request.FollowedById is not null)
        {
            context.FollowedBies.Add(new FollowedBy { Id = request.FollowedById.Value, Label = "FollowedBy", Value = "FollowedBy" });
        }

        context.SalesChannels.Add(new SalesChannel { Id = request.Booking.SalesChannelId, Label = "SalesChannel", Value = "SalesChannel", Language = "en" });
        context.SkissimTypes.Add(new SkissimType { Id = request.Booking.SkissimTypeId, Label = "SkissimType", Value = "SkissimType" });
        var serviceId = Guid.CreateVersion7();
        context.Services.Add(new Service { Id = serviceId, Label = "Service", Value = "Service" });
        context.Suppliers.Add(new Supplier { Id = request.Booking.Supplier.Id, Label = "Supplier", Value = "Supplier", SupplierAkioNumber = 1, ServiceId = serviceId });
        context.RefundStates.Add(new RefundState { Id = request.Compensation.RefundStateId, Label = "RefundState", Value = "RefundState" });
        context.CompensationReasons.Add(new CompensationReason { Id = request.Compensation.CompensationReasonId, Label = "CompensationReason", Value = "CompensationReason" });

        await context.SaveChangesAsync(cancellationToken);
    }

    internal static Claim CreateClaim(DateTimeOffset dateOfReceivedClaim, ClaimState? state = null)
    {
        var fixture = new Fixture();
        var claimId = Guid.CreateVersion7();
        var bookingId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var supplierId = Guid.CreateVersion7();
        var serviceId = Guid.CreateVersion7();
        var reasonId = Guid.CreateVersion7();
        var solutionId = Guid.CreateVersion7();

        return new Claim
        {
            Id = claimId,
            State = state ?? fixture.Create<ClaimState>(),
            ReasonId = reasonId,
            Reason = new Reason { Id = reasonId, Label = fixture.Create<string>(), Value = fixture.Create<string>() },
            SolutionId = solutionId,
            Solution = new Solution { Id = solutionId, Label = fixture.Create<string>(), Value = fixture.Create<string>() },
            BookingId = bookingId,
            Booking = new Booking
            {
                Id = bookingId,
                BookingNumber = fixture.Create<string>(),
                SalesChannelId = fixture.Create<Guid>(),
                SalesChannel = new SalesChannel { Id = Guid.CreateVersion7(), Label = fixture.Create<string>(), Value = fixture.Create<string>(), Language = fixture.Create<string>() },
                SkissimTypeId = fixture.Create<Guid>(),
                SkissimType = new SkissimType { Id = Guid.CreateVersion7(), Label = fixture.Create<string>(), Value = fixture.Create<string>() },
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
                    Label = fixture.Create<string>(),
                    Value = fixture.Create<string>(),
                    SupplierAkioNumber = fixture.Create<int>(),
                    ServiceId = serviceId,
                    Service = new Service { Id = serviceId, Label = fixture.Create<string>(), Value = fixture.Create<string>() }
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
                ClaimId = claimId,
                RefundStateId = fixture.Create<Guid>(),
                RefundState = new RefundState { Id = Guid.CreateVersion7(), Label = fixture.Create<string>(), Value = fixture.Create<string>() },
                CompensationReasonId = fixture.Create<Guid>(),
                CompensationReason = new CompensationReason { Id = Guid.CreateVersion7(), Label = fixture.Create<string>(), Value = fixture.Create<string>() }
            }
        };
    }

    internal static ClaimRequest CreateClaimRequest()
    {
        var fixture = new Fixture();

        return new ClaimRequest(
            State: fixture.Create<ClaimState>(),
            FollowedById: fixture.Create<Guid>(),
            ReasonId: fixture.Create<Guid>(),
            ClaimSummary: fixture.Create<string>(),
            SolutionId: fixture.Create<Guid>(),
            PurposeOfSolution: fixture.Create<string>(),
            UpdateReason: null,
            CustomerSuppInfo: fixture.Create<string>(),
            SupplierSuppInfo: fixture.Create<string>(),
            Booking: new BookingRequest(
                BookingNumber: fixture.Create<string>(),
                SalesChannelId: fixture.Create<Guid>(),
                Language: fixture.Create<Language>(),
                SkissimTypeId: fixture.Create<Guid>(),
                Product: fixture.Create<string>(),
                Customer: new CustomerRequest(
                    Name: fixture.Create<string>(),
                    AkioNumber: fixture.Create<int>()),
                Supplier: new SupplierRequest(
                    Id: fixture.Create<Guid>())),
            ClaimDate: new ClaimDateRequest(
                DateOfReceivedClaim: fixture.Create<DateTimeOffset>(),
                DateOfStartFollowUp: null,
                DateLastUpdate: null,
                DateOfDeparture: new DateTimeOffset(2025, 9, 15, 0, 0, 0, TimeSpan.Zero),
                DateEndOfFollowUp: null,
                DateOfArrival: new DateTimeOffset(2025, 8, 15, 0, 0, 0, TimeSpan.Zero)),
            Compensation: new CompensationRequest(
                CustomerVoucher: fixture.Create<float>(),
                CustomerUsedVoucher: null,
                SupplierRefund: null,
                ClaimRefund: null,
                RefundStateId: fixture.Create<Guid>(),
                CompensationReasonId: fixture.Create<Guid>()));
    }
}
