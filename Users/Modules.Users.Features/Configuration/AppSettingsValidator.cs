using Microsoft.Extensions.Options;

namespace Modules.Users.Features.Configuration;

internal sealed class AppSettingsValidator : IValidateOptions<AppSettings>
{
    public ValidateOptionsResult Validate(string? name, AppSettings options) =>
        Uri.TryCreate(options.PublicUrl, UriKind.Absolute, out var url) && url.Scheme == Uri.UriSchemeHttps
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail($"{AppSettings.SectionName}:{nameof(AppSettings.PublicUrl)} is required and must be an absolute https URL.");
}
