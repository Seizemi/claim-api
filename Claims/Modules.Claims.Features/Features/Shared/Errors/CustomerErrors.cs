namespace Modules.Claims.Features.Features.Shared.Errors;

internal static class CustomerErrorCodes
{
    internal const string CustomerCannotBeNull = "Customer.CannotBeNull";
    internal const string CustomerIdCannotBeEmptyGuid = "Customer.IdCannotBeEmptyGuid";
}

internal static class CustomerErrorMessages
{
    internal const string CustomerCannotBeNull = "Customer cannot be null.";
    internal const string CustomerIdCannotBeEmptyGuid = "Customer id cannot be empty.";
}
