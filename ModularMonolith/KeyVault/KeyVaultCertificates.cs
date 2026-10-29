using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using Azure.Security.KeyVault.Secrets;

namespace ModularMonolith.KeyVault;

/// <summary>
/// Certificates with their private key, read from Key Vault through their secret (PKCS#12) and kept in memory.
/// Key Vault renews them before they expire: reading them again regularly picks up the new version without a restart
/// (the app registrations must already trust it).
/// </summary>
internal sealed partial class KeyVaultCertificates(
    SecretClient secretClient,
    TimeProvider timeProvider,
    ILogger<KeyVaultCertificates> logger)
{
    internal static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(12);

    // Key Vault unreachable at refresh time: the previous certificate is kept, and read again after this delay.
    internal static readonly TimeSpan RetryInterval = TimeSpan.FromMinutes(5);

    private readonly ConcurrentDictionary<string, CachedCertificate> _certificates = new();
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    internal async Task<X509Certificate2> GetAsync(string name, CancellationToken cancellationToken)
    {
        if (TryGetFresh(name, out var certificate))
        {
            return certificate;
        }

        await _loadLock.WaitAsync(cancellationToken);
        try
        {
            if (TryGetFresh(name, out certificate))
            {
                return certificate;
            }

            try
            {
                KeyVaultSecret secret = await secretClient.GetSecretAsync(name, version: null, cancellationToken);
                // The previous version isn't disposed: a token request may still be signing with it.
                certificate = X509CertificateLoader.LoadPkcs12(
                    Convert.FromBase64String(secret.Value),
                    password: null,
                    OperatingSystem.IsMacOS() ? X509KeyStorageFlags.DefaultKeySet : X509KeyStorageFlags.EphemeralKeySet);
                _certificates[name] = new CachedCertificate(certificate, timeProvider.GetUtcNow() + RefreshInterval);

                return certificate;
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested
                && _certificates.TryGetValue(name, out var stale))
            {
                // Still valid for Entra until it expires: better than failing every sign-in while Key Vault is down.
                var retryAt = timeProvider.GetUtcNow() + RetryInterval;
                _certificates[name] = stale with { RefreshAt = retryAt };
                LogRefreshFailed(exception, name, retryAt);

                return stale.Certificate;
            }
        }
        finally
        {
            _loadLock.Release();
        }
    }

    private bool TryGetFresh(string name, out X509Certificate2 certificate)
    {
        if (_certificates.TryGetValue(name, out var cached) && timeProvider.GetUtcNow() < cached.RefreshAt)
        {
            certificate = cached.Certificate;
            return true;
        }

        certificate = null!;
        return false;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "The certificate {Name} could not be read again from Key Vault: the previous version is kept until {RetryAt}")]
    private partial void LogRefreshFailed(Exception exception, string name, DateTimeOffset retryAt);

    private sealed record CachedCertificate(X509Certificate2 Certificate, DateTimeOffset RefreshAt);
}
