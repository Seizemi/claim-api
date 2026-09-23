using FluentValidation;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;

namespace Modules.Claims.Features.Features.ReleaseClaimLock;

internal sealed class ReleaseClaimLockRequestValidator : AbstractValidator<ReleaseClaimLockRequest>
{
    public ReleaseClaimLockRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(ClaimErrorCodes.ClaimLockUserIdCannotBeEmpty)
            .WithMessage(ClaimErrorMessages.ClaimLockUserIdCannotBeEmpty);
    }
}
