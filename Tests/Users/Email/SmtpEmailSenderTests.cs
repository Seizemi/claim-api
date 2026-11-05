using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Modules.Users.Features.Configuration;
using Modules.Users.Features.Email;
using Moq;
using Xunit;

namespace Modules.Users.Features.Tests.Email;

public sealed class SmtpEmailSenderTests
{
    private static readonly WelcomeEmail Email = new("ann@example.com", "Ann Agent", "https://portal.example.com");

    private readonly Mock<IMailTransport> _transport = new();

    [Fact]
    public async Task SendWelcomeAsync_SendsTheWelcomeEmailToTheUserFromTheSender()
    {
        string? from = null, to = null, subject = null, text = null, html = null;
        _transport.Setup(transport => transport.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
            .Callback<MimeMessage, CancellationToken, ITransferProgress>((message, _, _) =>
            {
                from = message.From.Mailboxes.Single().ToString();
                to = message.To.Mailboxes.Single().Address;
                subject = message.Subject;
                text = message.TextBody;
                html = message.HtmlBody;
            })
            .ReturnsAsync("sent");

        await CreateSender(Settings()).SendWelcomeAsync(Email, TestContext.Current.CancellationToken);

        Assert.Equal("\"Claim\" <no-reply@example.com>", from);
        Assert.Equal(Email.To, to);
        var expected = WelcomeEmailContent.From(Email);
        Assert.Equal(expected.Subject, subject);
        Assert.Equal(expected.TextBody, text, ignoreLineEndingDifferences: true);
        Assert.Equal(expected.HtmlBody, html, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public async Task SendWelcomeAsync_ByDefault_RequiresTlsAndSignsIn()
    {
        await CreateSender(Settings()).SendWelcomeAsync(Email, TestContext.Current.CancellationToken);

        _transport.Verify(transport => transport.ConnectAsync("smtp.example.com", 587, SecureSocketOptions.StartTls, It.IsAny<CancellationToken>()));
        _transport.Verify(transport => transport.AuthenticateAsync("user", "secret", It.IsAny<CancellationToken>()));
        _transport.VerifySet(transport => transport.Timeout = (int)SmtpEmailSender.Timeout.TotalMilliseconds);
    }

    [Fact]
    public async Task SendWelcomeAsync_WithLocalServerSettings_NeitherUsesTlsNorSignsIn()
    {
        var settings = new EmailSettings { Host = "mailpit", Port = 1025, RequireTls = false, SenderAddress = "no-reply@example.com" };

        await CreateSender(settings).SendWelcomeAsync(Email, TestContext.Current.CancellationToken);

        _transport.Verify(transport => transport.ConnectAsync("mailpit", 1025, SecureSocketOptions.None, It.IsAny<CancellationToken>()));
        _transport.Verify(transport => transport.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendWelcomeAsync_WhenTheServerRefuses_FailsWithoutQuotingTheRecipient()
    {
        _transport.Setup(transport => transport.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
            .ThrowsAsync(new SmtpCommandException(
                SmtpErrorCode.RecipientNotAccepted,
                SmtpStatusCode.MailboxUnavailable,
                MailboxAddress.Parse(Email.To),
                $"Mailbox {Email.To} unavailable"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateSender(Settings()).SendWelcomeAsync(Email, TestContext.Current.CancellationToken));

        Assert.Contains("550", exception.Message);
        Assert.DoesNotContain(Email.To, exception.ToString());
    }

    private SmtpEmailSender CreateSender(EmailSettings settings) =>
        new(Options.Create(settings), () => _transport.Object);

    private static EmailSettings Settings() => new()
    {
        Host = "smtp.example.com",
        Username = "user",
        Password = "secret",
        SenderAddress = "no-reply@example.com",
    };
}
