using Microsoft.Extensions.Logging;

namespace Modules.Users.Features.Features.Shared.Logging;

/// <summary>
/// Audit events of user management: who, what, target, result. Never contains an email address.
/// </summary>
internal static partial class UserManagementAuditLog
{
    internal const string Create = "create";
    internal const string Delete = "delete";

    [LoggerMessage(Level = LogLevel.Information, Message = "Audit: {ActorUserId} {Action} user {TargetUserId}: succeeded")]
    internal static partial void AuditSucceeded(this ILogger logger, Guid actorUserId, string action, string targetUserId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Audit: {ActorUserId} {Action} user {TargetUserId}: rejected ({Reason})")]
    internal static partial void AuditRejected(this ILogger logger, Guid actorUserId, string action, string? targetUserId, string reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "Audit: {ActorUserId} {Action} user {TargetUserId}: failed")]
    internal static partial void AuditFailed(this ILogger logger, Exception exception, Guid actorUserId, string action, string? targetUserId);
}
