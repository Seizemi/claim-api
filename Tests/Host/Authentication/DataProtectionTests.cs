using Azure.Core;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModularMonolith.Authentication;
using ModularMonolith.Testing;
using Xunit;

namespace ModularMonolith.Tests.Authentication;

public sealed class DataProtectionTests : IDisposable
{
    private const string Purpose = "session-cookie-test";
    private const string BlobUri = "https://stclaimstaging.blob.core.windows.net/dataprotection/keys.xml";
    private const string KeyVaultKey = "https://kv-claim-staging.vault.azure.net/keys/dataprotection";

    private readonly DirectoryInfo _keysDirectory = Directory.CreateTempSubdirectory("claimapi-keys-");

    [Fact]
    public void AddBffAuthentication_WithKeysDirectory_WritesTheKeysThere()
    {
        using var provider = BuildProvider(_keysDirectory.FullName);

        provider.GetRequiredService<IDataProtectionProvider>().CreateProtector(Purpose).Protect("payload");

        Assert.NotEmpty(_keysDirectory.GetFiles("key-*.xml"));
    }

    [Fact]
    public void AddBffAuthentication_WithKeysDirectory_KeepsSessionsReadableAfterARestart()
    {
        string protectedPayload;
        using (var beforeRestart = BuildProvider(_keysDirectory.FullName))
        {
            protectedPayload = beforeRestart.GetRequiredService<IDataProtectionProvider>().CreateProtector(Purpose).Protect("payload");
        }

        using var afterRestart = BuildProvider(_keysDirectory.FullName);
        var payload = afterRestart.GetRequiredService<IDataProtectionProvider>().CreateProtector(Purpose).Unprotect(protectedPayload);

        Assert.Equal("payload", payload);
    }

    [Fact]
    public void AddBffAuthentication_WithBlobUriAndKeyVaultKey_StoresTheKeysInTheBlobWrappedByKeyVault()
    {
        using var provider = BuildProvider(new Dictionary<string, string?>
        {
            [AuthenticationExtensions.DataProtectionBlobUriKey] = BlobUri,
            [AuthenticationExtensions.DataProtectionKeyVaultKeyKey] = KeyVaultKey
        });

        // Resolving the options builds the clients without calling Azure.
        var options = provider.GetRequiredService<IOptions<KeyManagementOptions>>().Value;

        Assert.Equal("AzureBlobXmlRepository", options.XmlRepository?.GetType().Name);
        Assert.NotNull(options.XmlEncryptor);
    }

    [Theory]
    [InlineData(BlobUri, null)]
    [InlineData(null, KeyVaultKey)]
    public void AddBffAuthentication_WithOnlyOneOfBlobUriAndKeyVaultKey_Throws(string? blobUri, string? keyVaultKey)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => BuildProvider(new Dictionary<string, string?>
        {
            [AuthenticationExtensions.DataProtectionBlobUriKey] = blobUri,
            [AuthenticationExtensions.DataProtectionKeyVaultKeyKey] = keyVaultKey
        }));

        Assert.Contains("must be set together", exception.Message);
    }

    [Fact]
    public void AddBffAuthentication_WithBlobUriAndKeysDirectory_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => BuildProvider(new Dictionary<string, string?>
        {
            [AuthenticationExtensions.DataProtectionBlobUriKey] = BlobUri,
            [AuthenticationExtensions.DataProtectionKeyVaultKeyKey] = KeyVaultKey,
            [AuthenticationExtensions.DataProtectionKeysDirectoryKey] = _keysDirectory.FullName
        }));

        Assert.Contains("choose one key store", exception.Message);
    }

    public void Dispose() => _keysDirectory.Delete(recursive: true);

    private static ServiceProvider BuildProvider(string keysDirectory) =>
        BuildProvider(new Dictionary<string, string?> { [AuthenticationExtensions.DataProtectionKeysDirectoryKey] = keysDirectory });

    private static ServiceProvider BuildProvider(Dictionary<string, string?> dataProtectionSettings)
    {
        var settings = new Dictionary<string, string?>(BffTestHost.ValidSettings);
        foreach (var (key, value) in dataProtectionSettings)
        {
            settings[key] = value;
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<TokenCredential>(new UnusedCredential());
        services.AddBffAuthentication(configuration);

        return services.BuildServiceProvider();
    }

    private sealed class UnusedCredential : TokenCredential
    {
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("The tests don't call Azure.");

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("The tests don't call Azure.");
    }
}
