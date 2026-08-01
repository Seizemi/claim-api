namespace Modules.Claims.Features.Features.Shared.Errors;

internal static class ClaimErrorCodes
{
    internal const string ClaimCannotBeNull = "Claim.CannotBeNull";
    internal const string ClaimIdCannotBeEmpty = "Claim.IdCannotBeEmpty";
    internal const string ClaimBookingNumberCannotBeNullOrEmpty = "Claim.BookingNumberCannotBeNullOrEmpty";
    internal const string ClaimCustomerNameCannotBeNullOrEmpty = "Claim.CustomerNameCannotBeNullOrEmpty";
    internal const string ClaimSupplierIdCannotBeEmpty = "Claim.SupplierIdCannotBeEmpty";
    internal const string ClaimDateOfDepartureCannotBeSmallerThanDateOfArrival = "Claim.DateOfDepartureCannotBeSmallerThanDateOfArrival";
    internal const string ClaimDateOfArrivalCannotBeNull = "Claim.DateOfArrivalCannotBeNull";
    internal const string ClaimReasonIdCannotBeEmpty = "Claim.ReasonIdCannotBeEmpty";
    internal const string ClaimSolutionIdCannotBeEmpty = "Claim.SolutionIdCannotBeEmpty";
    internal const string ClaimSalesChannelIdCannotBeEmpty = "Claim.SalesChannelIdCannotBeEmpty";
    internal const string ClaimSkissimTypeIdCannotBeEmpty = "Claim.SkissimTypeIdCannotBeEmpty";
    internal const string ClaimRefundStateIdCannotBeEmpty = "Claim.RefundStateIdCannotBeEmpty";
    internal const string ClaimCompensationReasonIdCannotBeEmpty = "Claim.CompensationReasonIdCannotBeEmpty";
    internal const string ClaimFollowedByIdDoesNotExist = "Claim.FollowedByIdDoesNotExist";
    internal const string ClaimReasonIdDoesNotExist = "Claim.ReasonIdDoesNotExist";
    internal const string ClaimSolutionIdDoesNotExist = "Claim.SolutionIdDoesNotExist";
    internal const string ClaimSalesChannelIdDoesNotExist = "Claim.SalesChannelIdDoesNotExist";
    internal const string ClaimSkissimTypeIdDoesNotExist = "Claim.SkissimTypeIdDoesNotExist";
    internal const string ClaimSupplierIdDoesNotExist = "Claim.SupplierIdDoesNotExist";
    internal const string ClaimRefundStateIdDoesNotExist = "Claim.RefundStateIdDoesNotExist";
    internal const string ClaimCompensationReasonIdDoesNotExist = "Claim.CompensationReasonIdDoesNotExist";
}

internal static class ClaimErrorMessages
{
    internal const string ClaimCannotBeNull = "Claim doesn't exist.";
    internal const string ClaimIdCannotBeEmpty = "Claim id cannot be empty.";
    internal const string ClaimBookingNumberCannotBeNullOrEmpty = "Booking number cannot be null or empty.";
    internal const string ClaimCustomerNameCannotBeNullOrEmpty = "Customer name cannot be null or empty.";
    internal const string ClaimSupplierIdCannotBeEmpty = "Supplier id cannot be empty.";
    internal const string ClaimDateOfDepartureCannotBeSmallerThanDateOfArrival = "Date of departure cannot be later than date of arrival.";
    internal const string ClaimDateOfArrivalCannotBeNull = "Date of arrival cannot be null.";
    internal const string ClaimReasonIdCannotBeEmpty = "Reason id cannot be empty.";
    internal const string ClaimSolutionIdCannotBeEmpty = "Solution id cannot be empty.";
    internal const string ClaimSalesChannelIdCannotBeEmpty = "Sales channel id cannot be empty.";
    internal const string ClaimSkissimTypeIdCannotBeEmpty = "Skissim type id cannot be empty.";
    internal const string ClaimRefundStateIdCannotBeEmpty = "Refund state id cannot be empty.";
    internal const string ClaimCompensationReasonIdCannotBeEmpty = "Compensation reason id cannot be empty.";
    internal const string ClaimFollowedByIdDoesNotExist = "Followed by id does not exist.";
    internal const string ClaimReasonIdDoesNotExist = "Reason id does not exist.";
    internal const string ClaimSolutionIdDoesNotExist = "Solution id does not exist.";
    internal const string ClaimSalesChannelIdDoesNotExist = "Sales channel id does not exist.";
    internal const string ClaimSkissimTypeIdDoesNotExist = "Skissim type id does not exist.";
    internal const string ClaimSupplierIdDoesNotExist = "Supplier id does not exist.";
    internal const string ClaimRefundStateIdDoesNotExist = "Refund state id does not exist.";
    internal const string ClaimCompensationReasonIdDoesNotExist = "Compensation reason id does not exist.";
}
