using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Authentication;
using ModularMonolith.Testing;
using Xunit;

namespace ModularMonolith.Tests.Authentication;

public sealed class DataProtectionTests : IDisposable
{
    private const string Purpose = "session-cookie-test";

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

    public void Dispose() => _keysDirectory.Delete(recursive: true);

    private static ServiceProvider BuildProvider(string keysDirectory)
    {
        var settings = new Dictionary<string, string?>(BffTestHost.ValidSettings)
        {
            [AuthenticationExtensions.DataProtectionKeysDirectoryKey] = keysDirectory
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddBffAuthentication(configuration);

        return services.BuildServiceProvider();
    }
}
