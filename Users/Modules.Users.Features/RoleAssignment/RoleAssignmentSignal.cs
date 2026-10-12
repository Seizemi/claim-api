using System.Threading.Channels;

namespace Modules.Users.Features.RoleAssignment;

/// <summary>
/// Wakes the role assignment worker as soon as a user is created, instead of waiting for its next poll.
/// </summary>
internal sealed class RoleAssignmentSignal
{
    // One pending wake-up is enough: the worker processes every due row when it wakes.
    private readonly Channel<bool> _channel = Channel.CreateBounded<bool>(
        new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropWrite });

    public void Notify() => _channel.Writer.TryWrite(true);

    /// <summary>Returns when <see cref="Notify"/> is called or after <paramref name="timeout"/>.</summary>
    public async Task WaitAsync(TimeSpan timeout, TimeProvider timeProvider, CancellationToken cancellationToken)
    {
        using var timeoutSource = new CancellationTokenSource(timeout, timeProvider);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);
        try
        {
            await _channel.Reader.ReadAsync(linked.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Timed out: time for the regular poll.
        }
    }
}
