using FluentValidation;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;

namespace Modules.Claims.Features.Features.GetClaimsByState;

internal sealed class GetClaimsByStateRequestValidator : AbstractValidator<GetClaimsByStateRequest>
{
    public GetClaimsByStateRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode(PagingErrorCodes.PageNumberMustBeGreaterThanZero)
            .WithMessage(PagingErrorMessages.PageNumberMustBeGreaterThanZero);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, GetClaimsByStateHandler.MaxPageSize)
            .WithErrorCode(PagingErrorCodes.PageSizeMustBeBetweenOneAndMax)
            .WithMessage(PagingErrorMessages.PageSizeMustBeBetweenOneAndMax);
    }
}
