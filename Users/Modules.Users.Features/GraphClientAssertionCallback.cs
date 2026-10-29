namespace Modules.Users.Features;

/// <summary>
/// Signs a client assertion (JWT) for the Graph app registration, when the host holds its certificate (Key Vault in
/// Azure). Returns a JWT with issuer and subject <paramref name="clientId"/> and audience <paramref name="audience"/>.
/// </summary>
public delegate Task<string> GraphClientAssertionCallback(string clientId, string audience, CancellationToken cancellationToken);
