using Microsoft.Extensions.Options;
using ModularMonolith.Testing;
using Modules.Users.Features.Configuration;
using Modules.Users.Features.Email;
using Xunit;

namespace Modules.Users.Features.Tests.Configuration;

public sealed class UsersConfigurationTests
{
    private static readonly UserManagementSettingsValidator SettingsValidator = new();

    [Fact]
    public void Validate_WithAgentOnly_Succeeds()
    {
        var result = SettingsValidator.Validate(null, new UserManagementSettings { AssignableRoles = ["Agent"] });

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_WithSupervisor_FailsBecauseSupervisorsAreNeverAssignedFromTheApp()
    {
        var result = SettingsValidator.Validate(null, new UserManagementSettings { AssignableRoles = ["Agent", "Supervisor"] });

        Assert.True(result.Failed);
        Assert.Contains("must not contain Supervisor", result.FailureMessage);
    }

    [Fact]
    public void Validate_WithRoleUnknownToTheApplication_Fails()
    {
        var result = SettingsValidator.Validate(null, new UserManagementSettings { AssignableRoles = ["Auditor"] });

        Assert.True(result.Failed);
        Assert.Contains("'Auditor'", result.FailureMessage);
    }

    [Fact]
    public void Validate_WithNoAssignableRole_Fails()
    {
        var result = SettingsValidator.Validate(null, new UserManagementSettings());

        Assert.True(result.Failed);
    }

    [Theory]
    [InlineData("https://portal.example.com")]
    [InlineData("https://localhost:7001")]
    public void ValidatePublicUrl_WithAbsoluteHttpsUrl_Succeeds(string url)
    {
        var result = new AppSettingsValidator().Validate(null, new AppSettings { PublicUrl = url });

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("")]
    [InlineData("/relative")]
    [InlineData("http://portal.example.com")]
    public void ValidatePublicUrl_WithMissingRelativeOrHttpUrl_Fails(string url)
    {
        var result = new AppSettingsValidator().Validate(null, new AppSettings { PublicUrl = url });

        Assert.True(result.Failed);
        Assert.Contains("App:PublicUrl", result.FailureMessage);
    }

    [Fact]
    public async Task StartAsync_WithSupervisorAssignable_FailsAtStartup()
    {
        var settings = new Dictionary<string, string?>(BffTestHost.ValidSettings) { ["PartnerUsers:AssignableRoles:1"] = "Supervisor" };

        var exception = await Assert.ThrowsAsync<OptionsValidationException>(() => BffTestHost.StartAsync(settings));

        Assert.Contains("PartnerUsers:AssignableRoles", exception.Message);
    }

    [Fact]
    public async Task StartAsync_WithoutGraphCredential_FailsAtStartup()
    {
        var settings = new Dictionary<string, string?>(BffTestHost.ValidSettings) { ["PartnerUsers:GraphClientSecret"] = null };

        var exception = await Assert.ThrowsAsync<OptionsValidationException>(() => BffTestHost.StartAsync(settings));

        Assert.Contains("GraphClientSecret", exception.Message);
    }

    [Theory]
    [InlineData("ann.agent@example.com", "a***@example.com")]
    [InlineData("x@example.com", "x***@example.com")]
    [InlineData("no-at-sign", "***")]
    public void Mask_HidesTheLocalPart(string email, string expected)
    {
        Assert.Equal(expected, LoggingEmailSender.Mask(email));
    }
}
