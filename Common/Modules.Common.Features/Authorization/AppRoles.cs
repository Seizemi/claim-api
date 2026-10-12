namespace Modules.Common.Features.Authorization;

/// <summary>
/// App role values defined on the partner-portal-web registration in Entra External ID.
/// Renaming a role requires changing Entra, these constants and the frontend together. Role IDs are never configured:
/// the ClaimPartners.Users package reads them from Entra by these values.
/// </summary>
public static class AppRoles
{
    public const string Supervisor = "Supervisor";
    public const string Agent = "Agent";

    public static readonly IReadOnlyList<string> All = [Supervisor, Agent];
}
