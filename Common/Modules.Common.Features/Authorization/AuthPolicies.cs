namespace Modules.Common.Features.Authorization;

/// <summary>
/// Authorization policy names registered by the host. Modules reference them with
/// <c>.RequireAuthorization(AuthPolicies.Agent)</c> without depending on the host's authentication setup.
/// </summary>
public static class AuthPolicies
{
    /// <summary>User management. Satisfied only by the Supervisor role.</summary>
    public const string Supervisor = "Supervisor";

    /// <summary>Business features. Satisfied by the Agent and Supervisor roles.</summary>
    public const string Agent = "Agent";
}
