using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Infrastructure.Database;

namespace Modules.Claims.Features.Features.Shared.Validators;

internal static class ClaimLookupsValidator
{
    internal static async Task<List<Error>> ValidateLookupsExistAsync(this ClaimsDbContext context, ClaimRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        var followedByExists = request.FollowedById is null
            || await context.FollowedBies.AnyAsync(f => f.Id == request.FollowedById, cancellationToken);
        if (!followedByExists)
        {
            errors.Add(Error.Validation(ClaimErrorCodes.ClaimFollowedByIdDoesNotExist, ClaimErrorMessages.ClaimFollowedByIdDoesNotExist));
        }

        if (!await context.Reasons.AnyAsync(r => r.Id == request.ReasonId, cancellationToken))
        {
            errors.Add(Error.Validation(ClaimErrorCodes.ClaimReasonIdDoesNotExist, ClaimErrorMessages.ClaimReasonIdDoesNotExist));
        }

        if (!await context.Solutions.AnyAsync(s => s.Id == request.SolutionId, cancellationToken))
        {
            errors.Add(Error.Validation(ClaimErrorCodes.ClaimSolutionIdDoesNotExist, ClaimErrorMessages.ClaimSolutionIdDoesNotExist));
        }

        if (!await context.SalesChannels.AnyAsync(s => s.Id == request.Booking.SalesChannelId, cancellationToken))
        {
            errors.Add(Error.Validation(ClaimErrorCodes.ClaimSalesChannelIdDoesNotExist, ClaimErrorMessages.ClaimSalesChannelIdDoesNotExist));
        }

        if (!await context.SkissimTypes.AnyAsync(s => s.Id == request.Booking.SkissimTypeId, cancellationToken))
        {
            errors.Add(Error.Validation(ClaimErrorCodes.ClaimSkissimTypeIdDoesNotExist, ClaimErrorMessages.ClaimSkissimTypeIdDoesNotExist));
        }

        if (!await context.Services.AnyAsync(s => s.Id == request.Booking.Supplier.ServiceId, cancellationToken))
        {
            errors.Add(Error.Validation(ClaimErrorCodes.ClaimSupplierServiceIdDoesNotExist, ClaimErrorMessages.ClaimSupplierServiceIdDoesNotExist));
        }

        if (!await context.RefundStates.AnyAsync(r => r.Id == request.Compensation.RefundStateId, cancellationToken))
        {
            errors.Add(Error.Validation(ClaimErrorCodes.ClaimRefundStateIdDoesNotExist, ClaimErrorMessages.ClaimRefundStateIdDoesNotExist));
        }

        if (!await context.CompensationReasons.AnyAsync(c => c.Id == request.Compensation.CompensationReasonId, cancellationToken))
        {
            errors.Add(Error.Validation(ClaimErrorCodes.ClaimCompensationReasonIdDoesNotExist, ClaimErrorMessages.ClaimCompensationReasonIdDoesNotExist));
        }

        return errors;
    }
}
