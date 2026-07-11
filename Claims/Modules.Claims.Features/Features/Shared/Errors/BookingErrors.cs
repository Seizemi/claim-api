namespace Modules.Claims.Features.Features.Shared.Errors;

internal static class BookingErrorCodes
{
    internal const string BookingCannotBeNull = "Booking.CannotBeNull";
    internal const string BookingIdCannotBeEmptyGuid = "Booking.IdCannotBeEmptyGuid";
}

internal static class BookingErrorMessages
{
    internal const string BookingCannotBeNull = "Booking cannot be null.";
    internal const string BookingIdCannotBeEmptyGuid = "Booking id cannot be empty.";
}
