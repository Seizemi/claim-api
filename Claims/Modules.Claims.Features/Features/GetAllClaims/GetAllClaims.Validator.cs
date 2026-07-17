using FluentValidation;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;

namespace Modules.Claims.Features.Features.GetAllClaims;

internal sealed class GetAllClaimsRequestValidator : AbstractValidator<GetAllClaimsRequest>
{
    public GetAllClaimsRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode(PagingErrorCodes.PageNumberMustBeGreaterThanZero)
            .WithMessage(PagingErrorMessages.PageNumberMustBeGreaterThanZero);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, GetAllClaimsHandler.MaxPageSize)
            .WithErrorCode(PagingErrorCodes.PageSizeMustBeBetweenOneAndMax)
            .WithMessage(PagingErrorMessages.PageSizeMustBeBetweenOneAndMax);
    }
}
