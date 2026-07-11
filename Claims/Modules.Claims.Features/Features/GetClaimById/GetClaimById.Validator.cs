using FluentValidation;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;

namespace Modules.Claims.Features.Features.GetClaimById;

internal sealed class GetClaimByIdRequestValidator : AbstractValidator<GetClaimByIdRequest>
{
    public GetClaimByIdRequestValidator()
    {
        RuleFor(x => x.ClaimId)
            .NotEmpty()
            .WithErrorCode(ClaimErrorCodes.ClaimIdCannotBeEmpty)
            .WithMessage(ClaimErrorMessages.ClaimIdCannotBeEmpty);
    }
}
