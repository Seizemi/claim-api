using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using ModularMonolith.KeyVault;
using Xunit;

namespace ModularMonolith.Tests.KeyVault;

public sealed class ClientAssertionsTests
{
    private const string ClientId = "22222222-2222-2222-2222-222222222222";
    private const string Audience = "https://contoso.ciamlogin.com/11111111-1111-1111-1111-111111111111/oauth2/v2.0/token";

    private readonly X509Certificate2 _certificate = TestCertificates.Create();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 10, 28, 9, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task CreateAsync_SignsAJwtEntraCanCheckWithTheRegisteredCertificate()
    {
        var assertion = await CreateAssertionAsync();

        // Validated with the public key only, as Entra does with the certificate uploaded to the app registration.
        var result = await new JsonWebTokenHandler().ValidateTokenAsync(assertion, new TokenValidationParameters
        {
            ValidIssuer = ClientId,
            ValidAudience = Audience,
            IssuerSigningKey = new X509SecurityKey(X509CertificateLoader.LoadCertificate(_certificate.RawData)),
            LifetimeValidator = (_, _, _, _) => true
        });

        Assert.True(result.IsValid, result.Exception?.Message);
    }

    [Fact]
    public async Task CreateAsync_HasTheClaimsAndHeaderEntraRequires()
    {
        var token = new JsonWebToken(await CreateAssertionAsync());

        Assert.Equal(SecurityAlgorithms.RsaSha256, token.Alg);
        Assert.Equal(Base64UrlEncoder.Encode(_certificate.GetCertHash(HashAlgorithmName.SHA1)), token.X5t);
        Assert.Equal(ClientId, token.Issuer);
        Assert.Equal(ClientId, token.Subject);
        Assert.Equal([Audience], token.Audiences);
        Assert.True(Guid.TryParse(token.Id, out _));
        Assert.Equal(_timeProvider.GetUtcNow().UtcDateTime, token.ValidFrom);
        Assert.Equal(_timeProvider.GetUtcNow().UtcDateTime + ClientAssertions.Lifetime, token.ValidTo);
    }

    [Fact]
    public async Task CreateAsync_UsesANewIdEachTime()
    {
        var first = new JsonWebToken(await CreateAssertionAsync());
        var second = new JsonWebToken(await CreateAssertionAsync());

        Assert.NotEqual(first.Id, second.Id);
    }

    private Task<string> CreateAssertionAsync()
    {
        var certificates = new KeyVaultCertificates(TestCertificates.SecretClientServing(() => _certificate).Object, _timeProvider, NullLogger<KeyVaultCertificates>.Instance);
        return new ClientAssertions(certificates, _timeProvider)
            .CreateAsync(ClientId, "partner-portal-web", Audience, TestContext.Current.CancellationToken);
    }
}
