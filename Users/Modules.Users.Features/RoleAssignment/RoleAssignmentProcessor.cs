using ClaimPartners.Users.Core;
using ClaimPartners.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Enums;
using Modules.Users.Features.Configuration;
using Modules.Users.Features.Email;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Features.RoleAssignment;

internal interface IRoleAssignmentProcessor
{
    /// <summary>Makes one attempt for every due row.</summary>
    Task ProcessDueAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Assigns the role of users created moments ago (spec 5.8). Each row is leased before its attempt, so several
/// instances of the application never work on the same row; a lease left by a crashed instance simply expires.
/// </summary>
internal sealed partial class RoleAssignmentProcessor(
    UsersDbContext dbContext,
    IPartnerUserService partnerUsers,
    IEmailSender emailSender,
    IOptions<AppSettings> appSettings,
    TimeProvider timeProvider,
    ILogger<RoleAssignmentProcessor> logger) : IRoleAssignmentProcessor
{
    /// <summary>
    /// Wait after each failed attempt; once they are used up, the next failure is final (about 50 minutes after
    /// creation). Entra usually replicates a new user within seconds, sometimes more than a minute.
    /// </summary>
    internal static readonly IReadOnlyList<TimeSpan> RetryDelays =
    [
        TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(30)
    ];

    internal static readonly int MaxAttempts = RetryDelays.Count + 1;
    internal static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(2);
    internal const int BatchSize = 20;
    private const int LastErrorMaxLength = 500;

    public async Task ProcessDueAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var due = await dbContext.PendingRoleAssignments
            .Where(row => row.Status == RoleAssignmentStatus.Pending
                          && row.NextAttemptAt <= now
                          && (row.LockedUntil == null || row.LockedUntil <= now))
            .OrderBy(row => row.NextAttemptAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var row in due)
        {
            if (await TryLeaseAsync(row, cancellationToken))
            {
                await AttemptAsync(row, cancellationToken);
            }
        }
    }

    private async Task<bool> TryLeaseAsync(PendingRoleAssignment row, CancellationToken cancellationToken)
    {
        row.LockedUntil = timeProvider.GetUtcNow() + LeaseDuration;
        return await TrySaveAsync(row, cancellationToken);
    }

    private async Task AttemptAsync(PendingRoleAssignment row, CancellationToken cancellationToken)
    {
        try
        {
            await partnerUsers.AssignRoleAsync(row.UserId.ToString(), row.Role, cancellationToken);
        }
        catch (PartnerUserException exception)
        {
            await RecordFailureAsync(row, exception);
            return;
        }

        // The user can sign in from now on. A lost delete (database down) only means one more, idempotent, attempt.
        dbContext.PendingRoleAssignments.Remove(row);
        if (!await TrySaveAsync(row, CancellationToken.None))
        {
            // Deleted by a Supervisor meanwhile: the user is gone, no email.
            return;
        }

        LogRoleAssigned(row.UserId, row.Role, row.Attempts + 1, row.CreatedBy);
        await SendWelcomeEmailAsync(row);
    }

    private async Task RecordFailureAsync(PendingRoleAssignment row, PartnerUserException exception)
    {
        var now = timeProvider.GetUtcNow();
        row.Attempts++;
        row.LastError = Truncate($"{exception.Code}: {exception.InnerException?.Message ?? exception.Message}");
        row.LockedUntil = null;

        // A role disabled in Entra or an invalid ID won't fix itself.
        var final = row.Attempts >= MaxAttempts || exception.Code is not PartnerUserErrorCode.GraphError;
        if (final)
        {
            row.Status = RoleAssignmentStatus.Failed;
            LogAssignmentFailed(exception, row.UserId, row.Attempts);
        }
        else
        {
            row.NextAttemptAt = now + RetryDelays[row.Attempts - 1];
            LogAssignmentRetry(row.UserId, row.Attempts, row.NextAttemptAt, row.LastError);
        }

        // The outcome is saved even when the worker is stopping, or the attempt would be lost.
        await TrySaveAsync(row, CancellationToken.None);
    }

    /// <summary>Saves the row; false if another instance changed or deleted it first (the row is then left alone).</summary>
    private async Task<bool> TrySaveAsync(PendingRoleAssignment row, CancellationToken cancellationToken)
    {
        row.Version = Guid.NewGuid();
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.Entry(row).State = EntityState.Detached;
            return false;
        }
    }

    // A failed email is logged, never retried (spec D3).
    private async Task SendWelcomeEmailAsync(PendingRoleAssignment row)
    {
        try
        {
            await emailSender.SendWelcomeAsync(
                new WelcomeEmail(row.Email, row.DisplayName, appSettings.Value.PublicUrl),
                CancellationToken.None);
        }
        catch (Exception exception)
        {
            LogWelcomeEmailFailed(exception, row.UserId);
        }
    }

    private static string Truncate(string value) =>
        value.Length <= LastErrorMaxLength ? value : value[..LastErrorMaxLength];

    [LoggerMessage(Level = LogLevel.Information, Message = "Role {Role} assigned to user {UserId} after {Attempts} attempt(s) (created by {CreatedBy})")]
    private partial void LogRoleAssigned(Guid userId, string role, int attempts, Guid createdBy);

    [LoggerMessage(Level = LogLevel.Information, Message = "Role assignment attempt {Attempts} for user {UserId} failed, next attempt at {NextAttemptAt}: {Reason}")]
    private partial void LogAssignmentRetry(Guid userId, int attempts, DateTimeOffset nextAttemptAt, string reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "Role assignment for user {UserId} failed after {Attempts} attempt(s): giving up. The user is listed as Failed and can be deleted from the application.")]
    private partial void LogAssignmentFailed(Exception exception, Guid userId, int attempts);

    [LoggerMessage(Level = LogLevel.Error, Message = "The welcome email to user {UserId} could not be sent")]
    private partial void LogWelcomeEmailFailed(Exception exception, Guid userId);
}
