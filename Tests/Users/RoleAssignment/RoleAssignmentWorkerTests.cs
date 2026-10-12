using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Modules.Users.Features.RoleAssignment;
using Moq;
using Xunit;

namespace Modules.Users.Features.Tests.RoleAssignment;

public sealed class RoleAssignmentWorkerTests : IAsyncDisposable
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    private readonly Channel<bool> _passes = Channel.CreateUnbounded<bool>();
    private readonly Mock<IRoleAssignmentProcessor> _processor = new();
    private readonly RoleAssignmentSignal _signal = new();
    private readonly FakeTimeProvider _time = new();
    private readonly FakeLogger<RoleAssignmentWorker> _logger = new();
    private readonly ServiceProvider _services;
    private readonly RoleAssignmentWorker _worker;

    public RoleAssignmentWorkerTests()
    {
        _processor.Setup(processor => processor.ProcessDueAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _passes.Writer.TryWrite(true))
            .Returns(Task.CompletedTask);
        _services = new ServiceCollection().AddScoped(_ => _processor.Object).BuildServiceProvider();
        _worker = new RoleAssignmentWorker(_services.GetRequiredService<IServiceScopeFactory>(), _signal, _time, _logger);
    }

    [Fact]
    public async Task Start_ProcessesDueRowsAtOnce()
    {
        await _worker.StartAsync(TestContext.Current.CancellationToken);

        await NextPassAsync();
    }

    [Fact]
    public async Task Notify_WakesTheWorkerBeforeThePollInterval()
    {
        await _worker.StartAsync(TestContext.Current.CancellationToken);
        await NextPassAsync();

        _signal.Notify();

        await NextPassAsync();
    }

    [Fact]
    public async Task PollInterval_ProcessesAgainWithoutNotification()
    {
        await _worker.StartAsync(TestContext.Current.CancellationToken);
        await NextPassAsync();

        await AdvanceUntilNextPassAsync(RoleAssignmentWorker.PollInterval);
    }

    [Fact]
    public async Task FailedPass_IsLoggedAndTheWorkerKeepsRunning()
    {
        _processor.SetupSequence(processor => processor.ProcessDueAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                _passes.Writer.TryWrite(true);
                return Task.FromException(new InvalidOperationException("Database unreachable."));
            })
            .Returns(() =>
            {
                _passes.Writer.TryWrite(true);
                return Task.CompletedTask;
            });

        await _worker.StartAsync(TestContext.Current.CancellationToken);
        await NextPassAsync();

        await AdvanceUntilNextPassAsync(RoleAssignmentWorker.PollInterval);
        Assert.Contains(_logger.Collector.GetSnapshot(), record => record.Level == LogLevel.Error && record.Exception is InvalidOperationException);
    }

    public async ValueTask DisposeAsync()
    {
        await _worker.StopAsync(CancellationToken.None);
        _worker.Dispose();
        await _services.DisposeAsync();
    }

    private async Task NextPassAsync() =>
        await _passes.Reader.ReadAsync(TestContext.Current.CancellationToken).AsTask().WaitAsync(Timeout, TestContext.Current.CancellationToken);

    // The worker may not be waiting yet when the fake clock moves: keep moving it until the pass happens.
    private async Task AdvanceUntilNextPassAsync(TimeSpan step)
    {
        var deadline = DateTime.UtcNow + Timeout;
        while (!_passes.Reader.TryRead(out _))
        {
            Assert.True(DateTime.UtcNow < deadline, "The worker did not run again.");
            _time.Advance(step);
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }
    }
}
