using Microsoft.Extensions.Options;

namespace ModularMonolith.KeyVault;

internal sealed class KeyVaultSettingsValidator : IValidateOptions<KeyVaultSettings>
{
    public ValidateOptionsResult Validate(string? name, KeyVaultSettings options)
    {
        List<string> failures = [];

        if (!Uri.TryCreate(options.Uri, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            failures.Add($"{SettingKey(nameof(KeyVaultSettings.Uri))} must be an absolute https URL such as https://<vault>.vault.azure.net/.");
        }

        if (string.IsNullOrWhiteSpace(options.WebCertificateName))
        {
            failures.Add($"{SettingKey(nameof(KeyVaultSettings.WebCertificateName))} is required when {SettingKey(nameof(KeyVaultSettings.Uri))} is set.");
        }

        if (string.IsNullOrWhiteSpace(options.GraphCertificateName))
        {
            failures.Add($"{SettingKey(nameof(KeyVaultSettings.GraphCertificateName))} is required when {SettingKey(nameof(KeyVaultSettings.Uri))} is set.");
        }

        return failures.Count is 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static string SettingKey(string key) => $"{KeyVaultSettings.SectionName}:{key}";
}
