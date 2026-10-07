using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.Authentication;

internal sealed class SessionSettings
{
    internal const string SectionName = "Session";

    /// <summary>
    /// Absolute session lifetime counted from sign-in, whatever the user's activity (no sliding expiration).
    /// Fractional values are allowed so the expiry can be tested quickly (0.0834 is about 5 minutes).
    /// </summary>
    [Range(0.01, 24, ErrorMessage = "Session:LifetimeHours must be between 0.01 and 24.")]
    public double LifetimeHours { get; init; }
}
