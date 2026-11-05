using System.Net;

namespace Modules.Users.Features.Email;

/// <summary>
/// The welcome email, in French like the application. It never holds a password: the user sets one through
/// "Forgot password?" on the sign-in page (spec 7.3).
/// </summary>
internal sealed record WelcomeEmailContent(string Subject, string TextBody, string HtmlBody)
{
    internal static WelcomeEmailContent From(WelcomeEmail email)
    {
        var displayName = WebUtility.HtmlEncode(email.DisplayName);
        var appUrl = WebUtility.HtmlEncode(email.AppUrl);

        return new WelcomeEmailContent(
            "Votre compte a été créé",
            $"""
            Bonjour {email.DisplayName},

            Un compte vient d'être créé pour vous. Pour choisir votre mot de passe :

            1. Ouvrez {email.AppUrl}
            2. Sur la page de connexion, cliquez sur « Mot de passe oublié ? »
            3. Saisissez cette adresse e-mail, puis le code de vérification que vous recevrez, et choisissez votre mot de passe.
            """,
            $"""
            <p>Bonjour {displayName},</p>
            <p>Un compte vient d'être créé pour vous. Pour choisir votre mot de passe :</p>
            <ol>
              <li>Ouvrez <a href="{appUrl}">{appUrl}</a></li>
              <li>Sur la page de connexion, cliquez sur « Mot de passe oublié ? »</li>
              <li>Saisissez cette adresse e-mail, puis le code de vérification que vous recevrez, et choisissez votre mot de passe.</li>
            </ol>
            """);
    }
}
