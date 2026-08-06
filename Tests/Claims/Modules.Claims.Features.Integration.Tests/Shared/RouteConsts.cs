using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Features.Integration.Tests.Shared;

internal static class RouteConsts
{
    internal const string NewClaim = "/api/v1.0/Claim/new-claim/claim";
    internal const string DashboardClaim = "/api/v1.0/Claim/dashboard/claim";
    internal const string ClaimDetailsFormat = "/api/v1.0/Claim/claim-details/{0}/information";
    internal const string ClaimsByStateFormat = "/api/v1.0/Claim/by-state/{0}";
    internal const string ClaimsBySeasonFormat = "/api/v1.0/Claim/by-season/{0}";

    internal static string ClaimDetails(Guid claimId) => string.Format(ClaimDetailsFormat, claimId);
    internal static string ClaimsByState(ClaimState claimState) => string.Format(ClaimsByStateFormat, claimState);
    internal static string ClaimsBySeason(string seasonValue) => string.Format(ClaimsBySeasonFormat, seasonValue);
}
