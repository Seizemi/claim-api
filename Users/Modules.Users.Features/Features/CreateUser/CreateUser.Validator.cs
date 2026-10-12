using System.Net.Mail;
using FluentValidation;
using Microsoft.Extensions.Options;
using Modules.Users.Features.Configuration;
using Modules.Users.Features.Features.Shared.Errors;
using Modules.Users.Features.Features.Shared.Requests;

namespace Modules.Users.Features.Features.CreateUser;

internal sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    internal const int DisplayNameMaxLength = 100;
    private const int EmailMaxLength = 254;

    public CreateUserRequestValidator(IOptions<UserManagementSettings> settings)
    {
        var assignableRoles = settings.Value.AssignableRoles;

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode(UserErrorCodes.EmailCannotBeEmpty)
            .WithMessage(UserErrorMessages.EmailCannotBeEmpty)
            .Must(BeABareEmailAddress)
            .WithErrorCode(UserErrorCodes.EmailIsInvalid)
            .WithMessage(UserErrorMessages.EmailIsInvalid);

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithErrorCode(UserErrorCodes.DisplayNameCannotBeEmpty)
            .WithMessage(UserErrorMessages.DisplayNameCannotBeEmpty)
            .MaximumLength(DisplayNameMaxLength)
            .WithErrorCode(UserErrorCodes.DisplayNameTooLong)
            .WithMessage(UserErrorMessages.DisplayNameTooLong);

        RuleFor(x => x.Role)
            .Must(role => assignableRoles.Contains(role, StringComparer.Ordinal))
            .WithErrorCode(UserErrorCodes.RoleIsNotAssignable)
            .WithMessage(UserErrorMessages.RoleIsNotAssignable);
    }

    // Only "local@domain.tld": MailAddress alone also accepts "Name <local@domain>" and dotless hosts.
    private static bool BeABareEmailAddress(string email) =>
        email.Length <= EmailMaxLength
        && MailAddress.TryCreate(email, out var address)
        && address.Address == email
        && address.Host.Contains('.');
}
