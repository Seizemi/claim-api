namespace Modules.Claims.Features.Integration.Tests.Shared;

internal static class DateTimeOffsetAssert
{
    // Postgres 'timestamp with time zone' has microsecond precision; DateTimeOffset has 100ns
    // ticks, so an exact round-trip comparison can fail by a sub-microsecond amount.
    internal static void AreClose(DateTimeOffset? expected, DateTimeOffset? actual)
    {
        Assert.AreEqual(expected.HasValue, actual.HasValue);
        if (expected.HasValue && actual.HasValue)
        {
            Assert.IsTrue((expected.Value - actual.Value).Duration() < TimeSpan.FromMilliseconds(1));
        }
    }
}
