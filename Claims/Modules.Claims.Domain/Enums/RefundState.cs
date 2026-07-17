namespace Modules.Claims.Domain.Enums;

public enum RefundState
{
    Refunded,
    NotRefunded,
    AwaitingRib,
    AwaitingStripe
}
