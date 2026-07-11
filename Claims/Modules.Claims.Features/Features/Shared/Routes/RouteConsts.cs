namespace Modules.Claims.Features.Features.Shared.Routes;

internal static class RouteConsts
{
    internal const string BaseRoute = "/api/v1.0/Claim";
    internal const string NewClaim = $"{BaseRoute}/new-claim/claim";
    internal const string DashboardClaim = $"{BaseRoute}/dashboard/claim";
    internal const string ClaimDetails = $"{BaseRoute}/claim-details/{{claimId}}/information";
}
