namespace Modules.Claims.Features.Features.Shared.Errors;

internal static class PagingErrorCodes
{
    internal const string PageNumberMustBeGreaterThanZero = "Paging.PageNumberMustBeGreaterThanZero";
    internal const string PageSizeMustBeBetweenOneAndMax = "Paging.PageSizeMustBeBetweenOneAndMax";
}

internal static class PagingErrorMessages
{
    internal const string PageNumberMustBeGreaterThanZero = "Page number must be greater than zero.";
    internal const string PageSizeMustBeBetweenOneAndMax = "Page size must be between 1 and 100.";
}
