using ClaimPartners.Users.Domain;
using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace Modules.Users.Features.Features.Shared.Errors;

internal static class PartnerUserExceptionExtensions
{
    /// <summary>
    /// Converts the package's exception to the module's error. The exception message is never returned to the
    /// client: it may describe Graph internals.
    /// </summary>
    internal static Error ToError(this PartnerUserException exception) => exception.Code switch
    {
        PartnerUserErrorCode.UserAlreadyExists => Error.Conflict(UserErrorCodes.AlreadyExists, UserErrorMessages.AlreadyExists),
        PartnerUserErrorCode.UserNotFound => Error.NotFound(UserErrorCodes.NotFound, UserErrorMessages.NotFound),
        PartnerUserErrorCode.UnknownRole => Error.Validation(UserErrorCodes.RoleIsNotAssignable, UserErrorMessages.RoleIsNotAssignable),
        _ => Error.Custom(StatusCodes.Status502BadGateway, UserErrorCodes.DirectoryUnavailable, UserErrorMessages.DirectoryUnavailable)
    };
}
