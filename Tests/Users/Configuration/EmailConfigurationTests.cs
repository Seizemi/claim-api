using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModularMonolith.Testing;
using Modules.Users.Features.Configuration;
using Modules.Users.Features.Email;
using Xunit;

namespace Modules.Users.Features.Tests.Configuration;

public sealed class EmailConfigurationTests
{
    private static readonly EmailSettingsValidator Validator = new();

    [Fact]
    public void Validate_WithoutHost_Succeeds()
    {
        Assert.True(Validator.Validate(null, new EmailSettings()).Succeeded);
    }

    [Fact]
    public void Validate_WithCompleteSettings_Succeeds()
    {
        Assert.True(Validator.Validate(null, Valid()).Succeeded);
    }

    [Fact]
    public void Validate_WithLocalServerWithoutPasswordOrTls_Succeeds()
    {
        var settings = new EmailSettings { Host = "mailpit", Port = 1025, RequireTls = false, SenderAddress = "no-reply@example.com" };

        Assert.True(Validator.Validate(null, settings).Succeeded);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(65536)]
    public void Validate_WithPortOutOfRange_Fails(int port)
    {
        var result = Validator.Validate(null, Valid(port: port));

        Assert.True(result.Failed);
        Assert.Contains("Email:Port", result.FailureMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-address")]
    public void Validate_WithInvalidSenderAddress_Fails(string senderAddress)
    {
        var result = Validator.Validate(null, Valid(senderAddress: senderAddress));

        Assert.True(result.Failed);
        Assert.Contains("Email:SenderAddress", result.FailureMessage);
    }

    [Theory]
    [InlineData("user", "")]
    [InlineData("", "secret")]
    public void Validate_WithOnlyUsernameOrPassword_Fails(string username, string password)
    {
        var result = Validator.Validate(null, Valid(username: username, password: password));

        Assert.True(result.Failed);
        Assert.Contains("must be set together", result.FailureMessage);
    }

    [Fact]
    public void Validate_WithPasswordWithoutTls_Fails()
    {
        var result = Validator.Validate(null, Valid(requireTls: false));

        Assert.True(result.Failed);
        Assert.Contains("clear text", result.FailureMessage);
    }

    [Fact]
    public void AddUsersModule_WithoutHost_UsesTheLoggingSender()
    {
        using var provider = BuildUsersModule(BffTestHost.ValidSettings);

        Assert.IsType<LoggingEmailSender>(provider.GetRequiredService<IEmailSender>());
    }

    [Fact]
    public void AddUsersModule_WithHost_UsesTheSmtpSender()
    {
        var settings = new Dictionary<string, string?>(BffTestHost.ValidSettings)
        {
            ["Email:Host"] = "smtp.example.com",
            ["Email:Username"] = "user",
            ["Email:Password"] = "secret",
            ["Email:SenderAddress"] = "no-reply@example.com",
        };

        using var provider = BuildUsersModule(settings);

        Assert.IsType<SmtpEmailSender>(provider.GetRequiredService<IEmailSender>());
    }

    [Fact]
    public void AddUsersModule_WithInvalidEmailSettings_FailsStartupValidation()
    {
        var settings = new Dictionary<string, string?>(BffTestHost.ValidSettings) { ["Email:Host"] = "smtp.example.com" };

        using var provider = BuildUsersModule(settings);

        var exception = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IStartupValidator>().Validate());

        Assert.Contains("Email:SenderAddress", exception.Message);
    }

    private static EmailSettings Valid(
        int port = 587,
        string senderAddress = "no-reply@example.com",
        string username = "user",
        string password = "secret",
        bool requireTls = true) => new()
        {
            Host = "smtp.example.com",
            Port = port,
            SenderAddress = senderAddress,
            Username = username,
            Password = password,
            RequireTls = requireTls,
        };

    private static ServiceProvider BuildUsersModule(IReadOnlyDictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        return new ServiceCollection()
            .AddLogging()
            .AddSingleton<IConfiguration>(configuration)
            .AddUsersModule(configuration)
            .BuildServiceProvider();
    }
}
