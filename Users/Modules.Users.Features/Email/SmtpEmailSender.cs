using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Modules.Users.Features.Configuration;

namespace Modules.Users.Features.Email;

/// <summary>
/// Sends the emails through the SMTP server of <see cref="EmailSettings"/>, whichever provider runs it. One connection
/// per email: they are rare.
/// </summary>
internal sealed class SmtpEmailSender(IOptions<EmailSettings> settings, Func<IMailTransport> createTransport) : IEmailSender
{
    // The role assignment worker handles one row at a time and waits for this email.
    internal static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    public async Task SendWelcomeAsync(WelcomeEmail email, CancellationToken cancellationToken)
    {
        var options = settings.Value;
        using var message = CreateMessage(email, options);
        using var transport = createTransport();
        transport.Timeout = (int)Timeout.TotalMilliseconds;

        try
        {
            await transport.ConnectAsync(
                options.Host,
                options.Port,
                options.RequireTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                cancellationToken);

            if (!string.IsNullOrEmpty(options.Username))
            {
                await transport.AuthenticateAsync(options.Username, options.Password, cancellationToken);
            }

            await transport.SendAsync(message, cancellationToken);
            await transport.DisconnectAsync(quit: true, cancellationToken);
        }
        catch (SmtpCommandException exception)
        {
            // The server's message can quote the recipient, and the caller logs this exception: full email addresses
            // are never logged.
            throw new InvalidOperationException(
                $"The SMTP server refused the email ({exception.ErrorCode}, status {(int)exception.StatusCode}).");
        }
    }

    private static MimeMessage CreateMessage(WelcomeEmail email, EmailSettings options)
    {
        var content = WelcomeEmailContent.From(email);
        var message = new MimeMessage { Subject = content.Subject };
        message.From.Add(new MailboxAddress(options.SenderName, options.SenderAddress));
        message.To.Add(new MailboxAddress(email.DisplayName, email.To));
        message.Body = new BodyBuilder { TextBody = content.TextBody, HtmlBody = content.HtmlBody }.ToMessageBody();
        return message;
    }
}
