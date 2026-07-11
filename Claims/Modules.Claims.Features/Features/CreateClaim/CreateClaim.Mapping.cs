using Modules.Claims.Domain.Entities;
using Modules.Claims.Features.Features.Shared.Requests;

namespace Modules.Claims.Features.Features.CreateClaim;

internal static class CreateClaimMappingExtensions
{
    internal static Claim MapToClaim(this ClaimRequest request) => new()
    {
        Id = request.Id,
        State = request.State,
        FollowedBy = request.FollowedBy,
        Reason = request.Reason,
        ClaimSummary = request.ClaimSummary,
        Solution = request.Solution,
        PurposeOfSolution = request.PurposeOfSolution,
        UpdateReason = request.UpdateReason,
        CustomerSuppInfo = request.CustomerSuppInfo,
        SupplierSuppInfo = request.SupplierSuppInfo,
        BookingId = request.Booking.Id,
        Booking = request.Booking.MapToBooking(),
        ClaimDate = request.ClaimDate.MapToClaimDate(request.Id),
        Compensation = request.Compensation.MapToCompensation(request.Id)
    };

    private static Booking MapToBooking(this BookingRequest request) => new()
    {
        Id = request.Id,
        BookingNumber = request.BookingNumber,
        SalesChannel = request.SalesChannel,
        Language = request.Language,
        SeasonLabel = request.SeasonLabel,
        SeasonValue = request.SeasonValue,
        Service = request.Service,
        Skissim = request.Skissim,
        SkissimType = request.SkissimType,
        Product = request.Product,
        CustomerId = request.Customer.Id,
        Customer = request.Customer.MapToCustomer(),
        SupplierId = request.Supplier.Id,
        Supplier = request.Supplier.MapToSupplier()
    };

    private static Customer MapToCustomer(this CustomerRequest request) => new()
    {
        Id = request.Id,
        Name = request.Name,
        AkioNumber = request.AkioNumber
    };

    private static Supplier MapToSupplier(this SupplierRequest request) => new()
    {
        Id = request.Id,
        Name = request.Name,
        SupplierAkioNumber = request.SupplierAkioNumber
    };

    private static ClaimDate MapToClaimDate(this ClaimDateRequest request, Guid claimId) => new()
    {
        Id = Guid.NewGuid(),
        ClaimId = claimId,
        DateOfReceivedClaim = request.DateOfReceivedClaim,
        DateOfStartFollowUp = request.DateOfStartFollowUp,
        DateLastUpdate = request.DateLastUpdate,
        DateOfDeparture = request.DateOfDeparture,
        DateEndOfFollowUp = request.DateEndOfFollowUp,
        DateOfArrival = request.DateOfArrival
    };

    private static Compensation MapToCompensation(this CompensationRequest request, Guid claimId) => new()
    {
        Id = Guid.NewGuid(),
        ClaimId = claimId,
        CustomerVoucher = request.CustomerVoucher,
        CustomerUsedVoucher = request.CustomerUsedVoucher,
        SupplierRefund = request.SupplierRefund,
        ClaimRefound = request.ClaimRefound,
        RefoundState = request.RefoundState
    };
}
