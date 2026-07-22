using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Features.Features.Shared.Requests;

public sealed record GetClaimsByStateRequest(ClaimState ClaimState, int PageNumber = 1, int PageSize = 20);
