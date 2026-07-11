namespace Modules.Claims.Features.Features.Shared.Errors;

internal static class ClaimErrorCodes
{
    internal const string ClaimCannotBeNull = "Claim.CannotBeNull";
    internal const string ClaimIdCannotBeEmpty = "Claim.IdCannotBeEmpty";
    internal const string ClaimBookingNumberCannotBeNullOrEmpty = "Claim.BookingNumberCannotBeNullOrEmpty";
    internal const string ClaimCustomerNameCannotBeNullOrEmpty = "Claim.CustomerNameCannotBeNullOrEmpty";
    internal const string ClaimSupplierNameCannotBeNullOrEmpty = "Claim.SupplierNameCannotBeNullOrEmpty";
    internal const string ClaimDateOfDepartureCannotBeSmallerThanDateOfArrival = "Claim.DateOfDepartureCannotBeSmallerThanDateOfArrival";
    internal const string ClaimAlreadyExists = "Claim.AlreadyExists";
}

internal static class ClaimErrorMessages
{
    internal const string ClaimCannotBeNull = "Claim doesn't exist.";
    internal const string ClaimAlreadyExists = "A claim with this id already exists.";
    internal const string ClaimIdCannotBeEmpty = "Claim id cannot be empty.";
    internal const string ClaimBookingNumberCannotBeNullOrEmpty = "Booking number cannot be null or empty.";
    internal const string ClaimCustomerNameCannotBeNullOrEmpty = "Customer name cannot be null or empty.";
    internal const string ClaimSupplierNameCannotBeNullOrEmpty = "Supplier name cannot be null or empty.";
    internal const string ClaimDateOfDepartureCannotBeSmallerThanDateOfArrival = "Date of departure cannot be later than date of arrival.";
}
