namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record AcquireClaimLockResponse(
    bool Acquired,
    Guid? LockedByUserId,
    string? LockedByUserName,
    DateTimeOffset? LockedAt,
    ClaimResponse? Claim);
