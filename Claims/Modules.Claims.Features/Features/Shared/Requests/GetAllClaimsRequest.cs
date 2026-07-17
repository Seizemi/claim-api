namespace Modules.Claims.Features.Features.Shared.Requests;

public sealed record GetAllClaimsRequest(int PageNumber = 1, int PageSize = 20);
