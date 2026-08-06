namespace Modules.Claims.Features.Features.Shared.Errors;

internal static class SeasonErrorCodes
{
    internal const string SeasonValueIsInvalid = "Season.SeasonValueIsInvalid";
}

internal static class SeasonErrorMessages
{
    internal const string SeasonValueIsInvalid = "Season value is invalid. Expected format: 'ete{year}' or 'hiver{startYear}-{endYear}'.";
}
