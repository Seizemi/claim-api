namespace ModularMonolith.Testing;

/// <summary>A claim put in the test session cookie by <see cref="BffTestHost.SignInAsync"/>.</summary>
public sealed record TestClaim(string Type, string Value);
