using Microsoft.Extensions.Options;

namespace ModularMonolith.Authentication;

internal sealed class AzureAdSettingsValidator : IValidateOptions<AzureAdSettings>
{
    public ValidateOptionsResult Validate(string? name, AzureAdSettings options)
    {
        List<string> failures = [];

        AddFailureIfMissing(failures, nameof(AzureAdSettings.Authority), options.Authority);
        AddFailureIfMissing(failures, nameof(AzureAdSettings.TenantId), options.TenantId);
        AddFailureIfMissing(failures, nameof(AzureAdSettings.ClientId), options.ClientId);
        AddFailureIfMissing(failures, nameof(AzureAdSettings.ClientSecret), options.ClientSecret);

        if (IsSet(options.Authority)
            && (!Uri.TryCreate(options.Authority, UriKind.Absolute, out var authority)
                || authority.Scheme != Uri.UriSchemeHttps
                // "https://https://host/" is a valid URL (its host is "https"), but sign-in can't reach it.
                || options.Authority.IndexOf("://", StringComparison.Ordinal) != options.Authority.LastIndexOf("://", StringComparison.Ordinal)))
        {
            failures.Add($"{SettingKey(nameof(AzureAdSettings.Authority))} must be an absolute https URL such as https://<subdomain>.ciamlogin.com/.");
        }

        return failures.Count is 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void AddFailureIfMissing(List<string> failures, string key, string? value)
    {
        if (!IsSet(value))
        {
            // Never echo the value: it may be a secret.
            failures.Add($"{SettingKey(key)} is missing or still a placeholder. " +
                         $"For local development run, in ClaimApi/ModularMonolith: dotnet user-secrets set \"{SettingKey(key)}\" \"<value>\". " +
                         $"With docker compose, set {SettingKey(key).Replace(":", "__")} in ClaimApi/.env.local (see README).");
        }
    }

    private static bool IsSet(string? value) =>
        !string.IsNullOrWhiteSpace(value) && !value.Contains('<') && !value.Contains('>');

    private static string SettingKey(string key) => $"{AzureAdSettings.SectionName}:{key}";
}
