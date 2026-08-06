using FluentValidation;
using Modules.Claims.Domain;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;

namespace Modules.Claims.Features.Features.GetClaimsBySeason;

internal sealed class GetClaimsBySeasonRequestValidator : AbstractValidator<GetClaimsBySeasonRequest>
{
    public GetClaimsBySeasonRequestValidator()
    {
        RuleFor(x => x.SeasonValue)
            .Must(value => SeasonCalculator.TryResolveDateRange(value, out _, out _))
            .WithErrorCode(SeasonErrorCodes.SeasonValueIsInvalid)
            .WithMessage(SeasonErrorMessages.SeasonValueIsInvalid);
    }
}
