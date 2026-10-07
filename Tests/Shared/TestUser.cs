namespace ModularMonolith.Testing;

/// <summary>A signed-in user, as the session would describe it: Entra object ID, display name and app roles.</summary>
public sealed record TestUser(Guid Id, string Name, IReadOnlyList<string> Roles)
{
    public static readonly TestUser Alice = new(Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), "Alice", [TestRoles.Agent]);
    public static readonly TestUser Bob = new(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002"), "Bob", [TestRoles.Agent]);
}
