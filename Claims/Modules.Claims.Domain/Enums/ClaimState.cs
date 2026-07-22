namespace Modules.Claims.Domain.Enums;

public enum ClaimState
{
    AwaitingSupplier = 0,
    AwaitingClient = 1,
    Terminate = 2,
    ClosedWithoutResponse = 3
}
