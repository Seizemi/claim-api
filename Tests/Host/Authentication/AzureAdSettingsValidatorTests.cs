using Microsoft.Extensions.Options;
using ModularMonolith.Authentication;
using ModularMonolith.Testing;
using Xunit;

namespace ModularMonolith.Tests.Authentication;

public sealed class AzureAdSettingsValidatorTests
{
    private static readonly AzureAdSettingsValidator Validator = new();

    private static readonly AzureAdSettings ValidSettings = new()
    {
        Authority = "https://contoso.ciamlogin.com/",
        TenantId = "11111111-1111-1111-1111-111111111111",
        ClientId = "22222222-2222-2222-2222-222222222222",
        ClientSecret = "super-secret-value"
    };

    [Fact]
    public void Validate_WithAllValues_Succeeds()
    {
        var result = Validator.Validate(null, ValidSettings);

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(nameof(AzureAdSettings.Authority))]
    [InlineData(nameof(AzureAdSettings.TenantId))]
    [InlineData(nameof(AzureAdSettings.ClientId))]
    [InlineData(nameof(AzureAdSettings.ClientSecret))]
    public void Validate_WithMissingValue_FailsNamingTheKeyAndUserSecretsCommand(string key)
    {
        var settings = WithValue(key, null);

        var result = Validator.Validate(null, settings);

        Assert.True(result.Failed);
        var failure = Assert.Single(result.Failures!);
        Assert.Contains($"AzureAd:{key}", failure);
        Assert.Contains("dotnet user-secrets set", failure);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("<web-client-id>")]
    public void Validate_WithEmptyOrPlaceholderClientId_Fails(string clientId)
    {
        var settings = WithValue(nameof(AzureAdSettings.ClientId), clientId);

        var result = Validator.Validate(null, settings);

        Assert.True(result.Failed);
    }

    [Fact]
    public void Validate_WithPlaceholderAuthority_FailsOnlyAsMissing()
    {
        var settings = WithValue(nameof(AzureAdSettings.Authority), "https://<subdomain>.ciamlogin.com/");

        var result = Validator.Validate(null, settings);

        var failure = Assert.Single(result.Failures!);
        Assert.Contains("AzureAd:Authority is missing or still a placeholder", failure);
    }

    [Theory]
    [InlineData("http://contoso.ciamlogin.com/")]
    [InlineData("contoso.ciamlogin.com")]
    [InlineData("https://https://contoso.ciamlogin.com/")]
    public void Validate_WithNonHttpsAuthority_Fails(string authority)
    {
        var settings = WithValue(nameof(AzureAdSettings.Authority), authority);

        var result = Validator.Validate(null, settings);

        var failure = Assert.Single(result.Failures!);
        Assert.Contains("absolute https URL", failure);
    }

    [Fact]
    public void Validate_WithRejectedSecret_NeverEchoesItsValue()
    {
        const string rejectedSecret = "<my-real-secret-pasted-in-brackets>";
        var settings = WithValue(nameof(AzureAdSettings.ClientSecret), rejectedSecret);

        var result = Validator.Validate(null, settings);

        Assert.True(result.Failed);
        Assert.DoesNotContain(rejectedSecret, result.FailureMessage);
    }

    [Fact]
    public async Task StartAsync_WithoutAzureAdConfiguration_FailsWithClearMessage()
    {
        var settings = new Dictionary<string, string?> { ["Session:LifetimeHours"] = "8" };

        var exception = await Assert.ThrowsAsync<OptionsValidationException>(() => BffTestHost.StartAsync(settings));

        Assert.Contains("AzureAd:ClientId is missing", exception.Message);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("25")]
    public async Task StartAsync_WithOutOfRangeSessionLifetime_Fails(string lifetimeHours)
    {
        var settings = new Dictionary<string, string?>(BffTestHost.ValidSettings) { ["Session:LifetimeHours"] = lifetimeHours };

        var exception = await Assert.ThrowsAsync<OptionsValidationException>(() => BffTestHost.StartAsync(settings));

        Assert.Contains("Session:LifetimeHours", exception.Message);
    }

    private static AzureAdSettings WithValue(string key, string? value) => key switch
    {
        nameof(AzureAdSettings.Authority) => new AzureAdSettings { Authority = value, TenantId = ValidSettings.TenantId, ClientId = ValidSettings.ClientId, ClientSecret = ValidSettings.ClientSecret },
        nameof(AzureAdSettings.TenantId) => new AzureAdSettings { Authority = ValidSettings.Authority, TenantId = value, ClientId = ValidSettings.ClientId, ClientSecret = ValidSettings.ClientSecret },
        nameof(AzureAdSettings.ClientId) => new AzureAdSettings { Authority = ValidSettings.Authority, TenantId = ValidSettings.TenantId, ClientId = value, ClientSecret = ValidSettings.ClientSecret },
        nameof(AzureAdSettings.ClientSecret) => new AzureAdSettings { Authority = ValidSettings.Authority, TenantId = ValidSettings.TenantId, ClientId = ValidSettings.ClientId, ClientSecret = value },
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
    };
}
