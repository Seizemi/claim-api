using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Azure;
using Azure.Security.KeyVault.Secrets;
using Moq;

namespace ModularMonolith.Tests.KeyVault;

/// <summary>Self-signed certificates served by a fake Key Vault, the way Key Vault exposes them (PKCS#12 secret).</summary>
internal static class TestCertificates
{
    internal static X509Certificate2 Create(string subject = "CN=claim-test")
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddYears(1));
    }

    /// <summary>A SecretClient returning <paramref name="next"/>()'s certificate for every secret name.</summary>
    internal static Mock<SecretClient> SecretClientServing(Func<X509Certificate2> next)
    {
        var secretClient = new Mock<SecretClient>();
        secretClient
            .Setup(client => client.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string name, string _, CancellationToken _) => Response.FromValue(
                SecretModelFactory.KeyVaultSecret(
                    new SecretProperties(name) { ContentType = "application/x-pkcs12" },
                    Convert.ToBase64String(next().Export(X509ContentType.Pkcs12))),
                Mock.Of<Response>()));

        return secretClient;
    }
}
