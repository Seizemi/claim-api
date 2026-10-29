using ModularMonolith.KeyVault;
using Xunit;

namespace ModularMonolith.Tests.KeyVault;

public sealed class KeyVaultSettingsValidatorTests
{
    private static readonly KeyVaultSettingsValidator Validator = new();

    [Fact]
    public void Validate_WithAllValues_Succeeds()
    {
        var result = Validator.Validate(null, Settings());

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("kv-claim.vault.azure.net")]
    [InlineData("http://kv-claim.vault.azure.net/")]
    public void Validate_WithNonHttpsUri_Fails(string uri)
    {
        var result = Validator.Validate(null, Settings(uri: uri));

        Assert.Contains("KeyVault:Uri must be an absolute https URL", Assert.Single(result.Failures!));
    }

    [Theory]
    [InlineData(null, "partner-portal-graph", "KeyVault:WebCertificateName")]
    [InlineData("partner-portal-web", "", "KeyVault:GraphCertificateName")]
    public void Validate_WithoutACertificateName_FailsNamingTheKey(string? web, string? graph, string key)
    {
        var result = Validator.Validate(null, Settings(web: web, graph: graph));

        Assert.Contains($"{key} is required", Assert.Single(result.Failures!));
    }

    private static KeyVaultSettings Settings(
        string uri = "https://kv-claim.vault.azure.net/",
        string? web = "partner-portal-web",
        string? graph = "partner-portal-graph") =>
        new() { Uri = uri, WebCertificateName = web, GraphCertificateName = graph };
}
