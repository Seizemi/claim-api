using Microsoft.Extensions.Options;
using Modules.Common.Features.Authorization;

namespace Modules.Users.Features.Configuration;

internal sealed class UserManagementSettingsValidator : IValidateOptions<UserManagementSettings>
{
    private const string Key = $"{UserManagementSettings.SectionName}:{nameof(UserManagementSettings.AssignableRoles)}";

    public ValidateOptionsResult Validate(string? name, UserManagementSettings options)
    {
        var failures = new List<string>();

        if (options.AssignableRoles.Count == 0)
        {
            failures.Add($"{Key} must list at least one role.");
        }

        foreach (var role in options.AssignableRoles)
        {
            if (role == AppRoles.Supervisor)
            {
                failures.Add($"{Key} must not contain {AppRoles.Supervisor}: Supervisors are only assigned in the Entra admin center.");
            }
            else if (!AppRoles.All.Contains(role, StringComparer.Ordinal))
            {
                // Whether the role exists in Entra is only known on first use (the package reads the roles lazily).
                failures.Add($"{Key} contains '{role}', which is not an app role of the application ({string.Join(", ", AppRoles.All)}).");
            }
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
