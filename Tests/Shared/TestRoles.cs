using ModularMonolith.Authentication;

namespace ModularMonolith.Testing;

/// <summary>The app role values, for test projects that cannot see the host's internal <c>AppRoles</c>.</summary>
public static class TestRoles
{
    public const string Supervisor = AppRoles.Supervisor;
    public const string Agent = AppRoles.Agent;
}
