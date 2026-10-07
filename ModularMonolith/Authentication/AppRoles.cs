namespace ModularMonolith.Authentication;

/// <summary>
/// App role values defined on the partner-portal-web registration in Entra External ID.
/// Renaming a role requires changing Entra, these constants, the PartnerUsers:Roles keys and the frontend together.
/// </summary>
internal static class AppRoles
{
    internal const string Supervisor = "Supervisor";
    internal const string Agent = "Agent";
}
