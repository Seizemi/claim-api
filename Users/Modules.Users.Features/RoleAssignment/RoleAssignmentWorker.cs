using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Modules.Users.Features.RoleAssignment;

/// <summary>
/// Runs <see cref="IRoleAssignmentProcessor"/> when a user is created (<see cref="RoleAssignmentSignal"/>) and every
/// <see cref="PollInterval"/>, for the retries. Rows are in the database: a restart loses nothing.
/// </summary>
internal sealed partial class RoleAssignmentWorker(
    IServiceScopeFactory scopeFactory,
    RoleAssignmentSignal signal,
    TimeProvider timeProvider,
    ILogger<RoleAssignmentWorker> logger) : BackgroundService
{
    internal static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<IRoleAssignmentProcessor>().ProcessDueAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                // Database or Graph unreachable: the rows stay due, the next pass tries again.
                LogPassFailed(exception);
            }

            try
            {
                await signal.WaitAsync(PollInterval, timeProvider, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "A role assignment pass failed")]
    private partial void LogPassFailed(Exception exception);
}
