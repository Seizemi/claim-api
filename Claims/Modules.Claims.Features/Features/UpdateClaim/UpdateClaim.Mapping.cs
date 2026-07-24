using Modules.Claims.Domain.Entities;
using Modules.Claims.Features.Features.Shared.Requests;

namespace Modules.Claims.Features.Features.UpdateClaim;

internal static class UpdateClaimMappingExtensions
{
    internal static void UpdateFrom(this Claim claim, ClaimRequest request)
    {
        claim.State = request.State;
        claim.FollowedBy = request.FollowedBy;
        claim.Reason = request.Reason;
        claim.ClaimSummary = request.ClaimSummary;
        claim.Solution = request.Solution;
        claim.PurposeOfSolution = request.PurposeOfSolution;
        claim.UpdateReason = request.UpdateReason;
        claim.CustomerSuppInfo = request.CustomerSuppInfo;
        claim.SupplierSuppInfo = request.SupplierSuppInfo;

        claim.Booking.UpdateFrom(request.Booking);
        claim.ClaimDate.UpdateFrom(request.ClaimDate);
        claim.Compensation.UpdateFrom(request.Compensation);
    }

    private static void UpdateFrom(this Booking booking, BookingRequest request)
    {
        booking.BookingNumber = request.BookingNumber;
        booking.SalesChannel = request.SalesChannel;
        booking.Language = request.Language;
        booking.Service = request.Service;
        booking.Skissim = request.Skissim;
        booking.SkissimType = request.SkissimType;
        booking.Product = request.Product;

        booking.Customer.UpdateFrom(request.Customer);
        booking.Supplier.UpdateFrom(request.Supplier);
    }

    private static void UpdateFrom(this Customer customer, CustomerRequest request)
    {
        customer.Name = request.Name;
        customer.AkioNumber = request.AkioNumber;
    }

    private static void UpdateFrom(this Supplier supplier, SupplierRequest request)
    {
        supplier.Name = request.Name;
        supplier.SupplierAkioNumber = request.SupplierAkioNumber;
    }

    private static void UpdateFrom(this ClaimDate claimDate, ClaimDateRequest request)
    {
        claimDate.DateOfReceivedClaim = request.DateOfReceivedClaim;
        claimDate.DateOfStartFollowUp = request.DateOfStartFollowUp;
        claimDate.DateLastUpdate = request.DateLastUpdate;
        claimDate.DateOfDeparture = request.DateOfDeparture;
        claimDate.DateEndOfFollowUp = request.DateEndOfFollowUp;
        claimDate.DateOfArrival = request.DateOfArrival;
    }

    private static void UpdateFrom(this Compensation compensation, CompensationRequest request)
    {
        compensation.CustomerVoucher = request.CustomerVoucher;
        compensation.CustomerUsedVoucher = request.CustomerUsedVoucher;
        compensation.SupplierRefund = request.SupplierRefund;
        compensation.ClaimRefund = request.ClaimRefund;
        compensation.RefundState = request.RefundState;
    }
}
