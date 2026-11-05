using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Modules.Users.Features.Configuration;

internal sealed class EmailSettingsValidator : IValidateOptions<EmailSettings>
{
    public ValidateOptionsResult Validate(string? name, EmailSettings options)
    {
        if (!options.IsConfigured)
        {
            return ValidateOptionsResult.Success;
        }

        var failures = new List<string>();

        if (options.Port is < 1 or > 65535)
        {
            failures.Add($"{EmailSettings.SectionName}:{nameof(EmailSettings.Port)} must be between 1 and 65535.");
        }

        if (!MailAddress.TryCreate(options.SenderAddress, out _))
        {
            failures.Add($"{EmailSettings.SectionName}:{nameof(EmailSettings.SenderAddress)} is required and must be an email address.");
        }

        if (string.IsNullOrEmpty(options.Username) != string.IsNullOrEmpty(options.Password))
        {
            failures.Add($"{EmailSettings.SectionName}:{nameof(EmailSettings.Username)} and {nameof(EmailSettings.Password)} must be set together.");
        }

        if (!options.RequireTls && !string.IsNullOrEmpty(options.Password))
        {
            failures.Add($"{EmailSettings.SectionName}:{nameof(EmailSettings.RequireTls)} can't be false with a password: it would travel in clear text.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
