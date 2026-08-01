namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record PagedResponse(
    IReadOnlyList<ClaimSummaryResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);
