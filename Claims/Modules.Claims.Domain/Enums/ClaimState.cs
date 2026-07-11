namespace Modules.Claims.Domain.Enums;

public enum ClaimState
{
    AwaitingSupplier,
    AwaitingClient,
    Terminate,
    ClosedWithoutResponse
}
