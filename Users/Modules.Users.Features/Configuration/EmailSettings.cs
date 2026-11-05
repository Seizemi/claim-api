namespace Modules.Users.Features.Configuration;

/// <summary>The SMTP server sending the emails. Without a host, emails are only written to the log.</summary>
internal sealed class EmailSettings
{
    internal const string SectionName = "Email";

    public string Host { get; init; } = string.Empty;

    /// <summary>Submission port. Port 25 is blocked from App Service, and 465 (implicit TLS) isn't supported.</summary>
    public int Port { get; init; } = 587;

    /// <summary>Empty when the server takes no sign-in (Mailpit, locally).</summary>
    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    /// <summary>STARTTLS is mandatory unless turned off (Mailpit, locally).</summary>
    public bool RequireTls { get; init; } = true;

    public string SenderAddress { get; init; } = string.Empty;

    public string SenderName { get; init; } = "Claim";

    internal bool IsConfigured => !string.IsNullOrWhiteSpace(Host);
}
