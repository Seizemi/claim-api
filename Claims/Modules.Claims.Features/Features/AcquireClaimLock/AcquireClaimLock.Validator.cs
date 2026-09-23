using FluentValidation;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;

namespace Modules.Claims.Features.Features.AcquireClaimLock;

internal sealed class AcquireClaimLockRequestValidator : AbstractValidator<AcquireClaimLockRequest>
{
    public AcquireClaimLockRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(ClaimErrorCodes.ClaimLockUserIdCannotBeEmpty)
            .WithMessage(ClaimErrorMessages.ClaimLockUserIdCannotBeEmpty);

        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithErrorCode(ClaimErrorCodes.ClaimLockUserNameCannotBeNullOrEmpty)
            .WithMessage(ClaimErrorMessages.ClaimLockUserNameCannotBeNullOrEmpty);
    }
}
