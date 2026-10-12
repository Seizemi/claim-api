using Microsoft.Extensions.Logging;

namespace Modules.Users.Features.Email;

/// <summary>
/// Writes the welcome email to the log instead of sending it, until an email provider is chosen (spec D2).
/// The recipient is masked: full email addresses are never logged.
/// </summary>
internal sealed partial class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendWelcomeAsync(WelcomeEmail email, CancellationToken cancellationToken)
    {
        LogWelcomeEmail(logger, Mask(email.To), email.DisplayName, email.AppUrl);
        return Task.CompletedTask;
    }

    internal static string Mask(string email)
    {
        var at = email.IndexOf('@');
        return at <= 0 ? "***" : $"{email[0]}***{email[at..]}";
    }

    [LoggerMessage(Level = LogLevel.Information, Message =
        "Welcome email (logging sender, not sent) to {Recipient}: Hello {DisplayName}, an account was created for you. " +
        "Open {AppUrl}, choose \"Forgot password?\" on the sign-in page and set your password.")]
    private static partial void LogWelcomeEmail(ILogger logger, string recipient, string displayName, string appUrl);
}
