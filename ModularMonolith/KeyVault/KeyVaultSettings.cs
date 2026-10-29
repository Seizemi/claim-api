namespace ModularMonolith.KeyVault;

/// <summary>
/// The Key Vault holding the certificates of the two app registrations (Azure only). When <see cref="Uri"/> is set,
/// the web sign-in and Microsoft Graph authenticate with these certificates instead of client secrets.
/// </summary>
internal sealed class KeyVaultSettings
{
    internal const string SectionName = "KeyVault";

    public string? Uri { get; init; }
    public string? WebCertificateName { get; init; }
    public string? GraphCertificateName { get; init; }

    internal static bool IsConfigured(IConfiguration configuration) =>
        !string.IsNullOrWhiteSpace(configuration[$"{SectionName}:{nameof(Uri)}"]);
}
