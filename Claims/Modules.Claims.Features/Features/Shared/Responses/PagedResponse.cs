namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record PagedResponse(
    IReadOnlyList<ClaimResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);
