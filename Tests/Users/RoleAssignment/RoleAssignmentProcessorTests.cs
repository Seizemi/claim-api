using ClaimPartners.Users.Core;
using ClaimPartners.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Enums;
using Modules.Users.Features.Configuration;
using Modules.Users.Features.Email;
using Modules.Users.Features.RoleAssignment;
using Modules.Users.Features.Tests.Shared;
using Moq;
using Xunit;

namespace Modules.Users.Features.Tests.RoleAssignment;

public sealed class RoleAssignmentProcessorTests
{
    private const string PublicUrl = "https://portal.example.com";
    private static readonly Guid UserId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002");

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly Mock<IPartnerUserService> _partnerUsers = new();
    private readonly Mock<IEmailSender> _emailSender = new();
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 10, 9, 12, 0, 0, TimeSpan.Zero));
    private readonly FakeLogger<RoleAssignmentProcessor> _logger = new();

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private RoleAssignmentProcessor CreateProcessor() => new(
        UsersDbContextFactory.Create(_databaseName),
        _partnerUsers.Object,
        _emailSender.Object,
        Options.Create(new AppSettings { PublicUrl = PublicUrl }),
        _time,
        _logger);

    [Fact]
    public async Task ProcessDueAsync_WhenAssigned_DeletesRowAndSendsWelcomeEmail()
    {
        await SeedAsync(DueRow());

        await CreateProcessor().ProcessDueAsync(Token);

        _partnerUsers.Verify(service => service.AssignRoleAsync(UserId.ToString(), "Agent", It.IsAny<CancellationToken>()), Times.Once);
        Assert.Null(await FindAsync());
        _emailSender.Verify(sender => sender.SendWelcomeAsync(
            new WelcomeEmail("ann@example.com", "Ann Agent", PublicUrl), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Contains(_logger.Collector.GetSnapshot(), record =>
            record.Level == LogLevel.Information && record.Message.Contains(UserId.ToString()) && !record.Message.Contains("ann@example.com"));
    }

    [Fact]
    public async Task ProcessDueAsync_DuringTheAttempt_HoldsALeaseOnTheRow()
    {
        await SeedAsync(DueRow());
        DateTimeOffset? lockedUntil = null;
        _partnerUsers.Setup(service => service.AssignRoleAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(async () => lockedUntil = (await FindAsync())!.LockedUntil);

        await CreateProcessor().ProcessDueAsync(Token);

        Assert.Equal(_time.GetUtcNow() + RoleAssignmentProcessor.LeaseDuration, lockedUntil);
    }

    [Fact]
    public async Task ProcessDueAsync_WithRowNotDueYet_DoesNothing()
    {
        await SeedAsync(DueRow(nextAttemptAt: _time.GetUtcNow().AddSeconds(1)));

        await CreateProcessor().ProcessDueAsync(Token);

        _partnerUsers.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessDueAsync_WithRowLeasedByAnotherInstance_LeavesIt()
    {
        var row = DueRow();
        row.LockedUntil = _time.GetUtcNow().AddMinutes(1);
        await SeedAsync(row);

        await CreateProcessor().ProcessDueAsync(Token);

        _partnerUsers.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessDueAsync_WithExpiredLease_TakesTheRow()
    {
        var row = DueRow();
        row.LockedUntil = _time.GetUtcNow().AddSeconds(-1);
        await SeedAsync(row);

        await CreateProcessor().ProcessDueAsync(Token);

        Assert.Null(await FindAsync());
    }

    [Fact]
    public async Task ProcessDueAsync_WithFailedRow_LeavesIt()
    {
        await SeedAsync(UsersDbContextFactory.PendingAssignment(UserId, status: RoleAssignmentStatus.Failed, nextAttemptAt: _time.GetUtcNow()));

        await CreateProcessor().ProcessDueAsync(Token);

        _partnerUsers.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessDueAsync_WhenGraphFails_SchedulesTheNextAttemptAndReleasesTheLease()
    {
        await SeedAsync(DueRow());
        GivenAssignmentFails(PartnerUserErrorCode.GraphError, "Not a valid reference update.");

        await CreateProcessor().ProcessDueAsync(Token);

        var row = await FindAsync();
        Assert.NotNull(row);
        Assert.Equal(RoleAssignmentStatus.Pending, row.Status);
        Assert.Equal(1, row.Attempts);
        Assert.Equal(_time.GetUtcNow() + RoleAssignmentProcessor.RetryDelays[0], row.NextAttemptAt);
        Assert.Null(row.LockedUntil);
        Assert.Contains("Not a valid reference update.", row.LastError);
        _emailSender.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessDueAsync_WhenGraphFailsAgain_WaitsLongerEachTime()
    {
        await SeedAsync(DueRow(attempts: 3));
        GivenAssignmentFails(PartnerUserErrorCode.GraphError);

        await CreateProcessor().ProcessDueAsync(Token);

        var row = await FindAsync();
        Assert.Equal(4, row!.Attempts);
        Assert.Equal(_time.GetUtcNow() + RoleAssignmentProcessor.RetryDelays[3], row.NextAttemptAt);
    }

    [Fact]
    public async Task ProcessDueAsync_WhenTheLastAttemptFails_MarksTheRowFailedAndLogsAnError()
    {
        await SeedAsync(DueRow(attempts: RoleAssignmentProcessor.MaxAttempts - 1));
        GivenAssignmentFails(PartnerUserErrorCode.GraphError);

        await CreateProcessor().ProcessDueAsync(Token);

        var row = await FindAsync();
        Assert.Equal(RoleAssignmentStatus.Failed, row!.Status);
        Assert.Equal(RoleAssignmentProcessor.MaxAttempts, row.Attempts);
        Assert.Contains(_logger.Collector.GetSnapshot(), record => record.Level == LogLevel.Error && record.Message.Contains(UserId.ToString()));
    }

    [Theory]
    [InlineData(PartnerUserErrorCode.UnknownRole)]
    [InlineData(PartnerUserErrorCode.UserNotFound)]
    public async Task ProcessDueAsync_WhenTheFailureCannotFixItself_MarksTheRowFailedAtOnce(PartnerUserErrorCode code)
    {
        await SeedAsync(DueRow());
        GivenAssignmentFails(code);

        await CreateProcessor().ProcessDueAsync(Token);

        var row = await FindAsync();
        Assert.Equal(RoleAssignmentStatus.Failed, row!.Status);
        Assert.Equal(1, row.Attempts);
    }

    [Fact]
    public void RetryDelays_GiveUpAboutFiftyMinutesAfterCreation()
    {
        var total = RoleAssignmentProcessor.RetryDelays.Aggregate(TimeSpan.Zero, (sum, delay) => sum + delay);

        Assert.InRange(total, TimeSpan.FromMinutes(45), TimeSpan.FromMinutes(55));
    }

    [Fact]
    public async Task ProcessDueAsync_WhenWelcomeEmailFails_StillDeletesTheRowAndLogsAnError()
    {
        await SeedAsync(DueRow());
        _emailSender.Setup(sender => sender.SendWelcomeAsync(It.IsAny<WelcomeEmail>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP down"));

        await CreateProcessor().ProcessDueAsync(Token);

        Assert.Null(await FindAsync());
        Assert.Contains(_logger.Collector.GetSnapshot(), record => record.Level == LogLevel.Error && record.Exception is InvalidOperationException);
    }

    [Fact]
    public async Task ProcessDueAsync_WhenTheUserIsDeletedDuringTheAttempt_SendsNoEmail()
    {
        await SeedAsync(DueRow());
        _partnerUsers.Setup(service => service.AssignRoleAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                // A Supervisor deletes the pending user meanwhile (DeleteUser handler).
                await using var other = UsersDbContextFactory.Create(_databaseName);
                other.PendingRoleAssignments.Remove((await other.PendingRoleAssignments.FindAsync([UserId], Token))!);
                await other.SaveChangesAsync(Token);
            });

        await CreateProcessor().ProcessDueAsync(Token);

        _emailSender.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessDueAsync_WithSeveralDueRows_AttemptsEachOne()
    {
        await SeedAsync(UsersDbContextFactory.PendingAssignment(), UsersDbContextFactory.PendingAssignment());

        await CreateProcessor().ProcessDueAsync(Token);

        _partnerUsers.Verify(service => service.AssignRoleAsync(It.IsAny<string>(), "Agent", It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    private PendingRoleAssignment DueRow(DateTimeOffset? nextAttemptAt = null, int attempts = 0) =>
        UsersDbContextFactory.PendingAssignment(UserId, nextAttemptAt: nextAttemptAt ?? _time.GetUtcNow(), attempts: attempts);

    private void GivenAssignmentFails(PartnerUserErrorCode code, string? graphMessage = null) =>
        _partnerUsers.Setup(service => service.AssignRoleAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PartnerUserException(
                code,
                "Microsoft Graph call failed (HTTP 400, Request_BadRequest).",
                graphMessage is null ? null : new InvalidOperationException(graphMessage)));

    private async Task SeedAsync(params PendingRoleAssignment[] rows)
    {
        await using var seed = UsersDbContextFactory.Create(_databaseName);
        seed.PendingRoleAssignments.AddRange(rows);
        await seed.SaveChangesAsync(Token);
    }

    private async Task<PendingRoleAssignment?> FindAsync()
    {
        await using var read = UsersDbContextFactory.Create(_databaseName);
        return await read.PendingRoleAssignments.AsNoTracking().SingleOrDefaultAsync(row => row.UserId == UserId, Token);
    }
}
