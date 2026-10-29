using System.Security.Cryptography.X509Certificates;
using Azure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using ModularMonolith.KeyVault;
using Moq;
using Xunit;

namespace ModularMonolith.Tests.KeyVault;

public sealed class KeyVaultCertificatesTests
{
    private readonly FakeTimeProvider _timeProvider = new(DateTimeOffset.UtcNow);
    private readonly DateTimeOffset _start;

    public KeyVaultCertificatesTests() => _start = _timeProvider.GetUtcNow();

    [Fact]
    public async Task GetAsync_LoadsTheCertificateWithItsPrivateKey()
    {
        var expected = TestCertificates.Create();
        var certificates = new KeyVaultCertificates(TestCertificates.SecretClientServing(() => expected).Object, _timeProvider, NullLogger<KeyVaultCertificates>.Instance);

        var certificate = await certificates.GetAsync("partner-portal-web", TestContext.Current.CancellationToken);

        Assert.Equal(expected.Thumbprint, certificate.Thumbprint);
        Assert.True(certificate.HasPrivateKey);
    }

    [Fact]
    public async Task GetAsync_BeforeTheRefreshInterval_ReadsKeyVaultOnce()
    {
        var secretClient = TestCertificates.SecretClientServing(() => TestCertificates.Create());
        var certificates = new KeyVaultCertificates(secretClient.Object, _timeProvider, NullLogger<KeyVaultCertificates>.Instance);

        var first = await certificates.GetAsync("partner-portal-web", TestContext.Current.CancellationToken);
        _timeProvider.Advance(KeyVaultCertificates.RefreshInterval - TimeSpan.FromMinutes(1));
        var second = await certificates.GetAsync("partner-portal-web", TestContext.Current.CancellationToken);

        Assert.Same(first, second);
        secretClient.Verify(client => client.GetSecretAsync("partner-portal-web", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_AfterTheRefreshInterval_PicksUpTheRenewedCertificate()
    {
        var current = TestCertificates.Create("CN=before-renewal");
        var certificates = new KeyVaultCertificates(TestCertificates.SecretClientServing(() => current).Object, _timeProvider, NullLogger<KeyVaultCertificates>.Instance);
        await certificates.GetAsync("partner-portal-web", TestContext.Current.CancellationToken);

        current = TestCertificates.Create("CN=after-renewal");
        _timeProvider.Advance(KeyVaultCertificates.RefreshInterval);
        var renewed = await certificates.GetAsync("partner-portal-web", TestContext.Current.CancellationToken);

        Assert.Equal(current.Thumbprint, renewed.Thumbprint);
    }

    [Fact]
    public async Task GetAsync_CachesEachCertificateByName()
    {
        var secretClient = TestCertificates.SecretClientServing(() => TestCertificates.Create());
        var certificates = new KeyVaultCertificates(secretClient.Object, _timeProvider, NullLogger<KeyVaultCertificates>.Instance);

        var web = await certificates.GetAsync("partner-portal-web", TestContext.Current.CancellationToken);
        var graph = await certificates.GetAsync("partner-portal-graph", TestContext.Current.CancellationToken);

        Assert.NotEqual(web.Thumbprint, graph.Thumbprint);
    }

    [Fact]
    public async Task GetAsync_FirstLoadFails_Throws()
    {
        var certificates = CertificatesServing(() => throw KeyVaultUnavailable());

        await Assert.ThrowsAsync<RequestFailedException>(() =>
            certificates.GetAsync("partner-portal-web", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetAsync_RefreshFails_ReturnsThePreviousCertificate()
    {
        var keyVaultDown = false;
        var previous = TestCertificates.Create();
        var certificates = CertificatesServing(() => keyVaultDown ? throw KeyVaultUnavailable() : previous);
        await certificates.GetAsync("partner-portal-web", TestContext.Current.CancellationToken);

        keyVaultDown = true;
        _timeProvider.Advance(KeyVaultCertificates.RefreshInterval);
        var certificate = await certificates.GetAsync("partner-portal-web", TestContext.Current.CancellationToken);

        Assert.Equal(previous.Thumbprint, certificate.Thumbprint);
    }

    [Fact]
    public async Task GetAsync_RefreshFails_ReadsKeyVaultAgainAfterTheRetryInterval()
    {
        var keyVaultDown = false;
        var current = TestCertificates.Create("CN=before-renewal");
        var secretClient = TestCertificates.SecretClientServing(() => keyVaultDown ? throw KeyVaultUnavailable() : current);
        var certificates = new KeyVaultCertificates(secretClient.Object, _timeProvider, NullLogger<KeyVaultCertificates>.Instance);
        await certificates.GetAsync("partner-portal-web", TestContext.Current.CancellationToken);
        keyVaultDown = true;
        _timeProvider.Advance(KeyVaultCertificates.RefreshInterval);
        await certificates.GetAsync("partner-portal-web", TestContext.Current.CancellationToken);

        keyVaultDown = false;
        current = TestCertificates.Create("CN=after-renewal");
        _timeProvider.Advance(KeyVaultCertificates.RetryInterval - TimeSpan.FromSeconds(1));
        var beforeRetry = await certificates.GetAsync("partner-portal-web", TestContext.Current.CancellationToken);
        _timeProvider.Advance(TimeSpan.FromSeconds(1));
        var afterRetry = await certificates.GetAsync("partner-portal-web", TestContext.Current.CancellationToken);

        Assert.NotEqual(current.Thumbprint, beforeRetry.Thumbprint);
        Assert.Equal(current.Thumbprint, afterRetry.Thumbprint);
        secretClient.Verify(client => client.GetSecretAsync("partner-portal-web", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task GetAsync_CancelledDuringRefresh_ThrowsInsteadOfReturningThePreviousCertificate()
    {
        using var cancellation = new CancellationTokenSource();
        var certificates = CertificatesServing(() =>
        {
            if (_timeProvider.GetUtcNow() > _start)
            {
                cancellation.Cancel();
                throw new OperationCanceledException(cancellation.Token);
            }

            return TestCertificates.Create();
        });
        await certificates.GetAsync("partner-portal-web", cancellation.Token);

        _timeProvider.Advance(KeyVaultCertificates.RefreshInterval);

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            certificates.GetAsync("partner-portal-web", cancellation.Token));
    }

    private KeyVaultCertificates CertificatesServing(Func<X509Certificate2> next) =>
        new(TestCertificates.SecretClientServing(next).Object, _timeProvider, NullLogger<KeyVaultCertificates>.Instance);

    private static RequestFailedException KeyVaultUnavailable() => new(503, "Service Unavailable");
}
