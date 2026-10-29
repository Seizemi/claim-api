using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ModularMonolith.KeyVault;

/// <summary>
/// Client assertions (RFC 7523): short JWTs signed with an app registration's certificate, sent to Entra instead of
/// a client secret. Entra checks the signature with the certificate registered on the app (header <c>x5t</c>).
/// </summary>
internal sealed class ClientAssertions(KeyVaultCertificates certificates, TimeProvider timeProvider)
{
    internal const string AssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";

    internal static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);

    private static readonly JsonWebTokenHandler TokenHandler = new();

    internal async Task<string> CreateAsync(string clientId, string certificateName, string audience, CancellationToken cancellationToken)
    {
        var certificate = await certificates.GetAsync(certificateName, cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        return TokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = clientId,
            Audience = audience,
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Sub, clientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ]),
            IssuedAt = now,
            NotBefore = now,
            Expires = now + Lifetime,
            SigningCredentials = new X509SigningCredentials(certificate)
        });
    }
}
