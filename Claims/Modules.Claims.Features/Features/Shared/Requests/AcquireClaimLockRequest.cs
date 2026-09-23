namespace Modules.Claims.Features.Features.Shared.Requests;

public sealed record AcquireClaimLockRequest(Guid UserId, string UserName);
