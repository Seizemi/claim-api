namespace Modules.Claims.Domain.Enums;

public enum RefundState
{
    Refunded = 0,
    NotRefunded = 1,
    AwaitingRib = 2,
    AwaitingStripe = 3
}
