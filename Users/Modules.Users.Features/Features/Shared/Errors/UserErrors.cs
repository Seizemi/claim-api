namespace Modules.Users.Features.Features.Shared.Errors;

internal static class UserErrorCodes
{
    internal const string EmailCannotBeEmpty = "User.EmailCannotBeEmpty";
    internal const string EmailIsInvalid = "User.EmailIsInvalid";
    internal const string DisplayNameCannotBeEmpty = "User.DisplayNameCannotBeEmpty";
    internal const string DisplayNameTooLong = "User.DisplayNameTooLong";
    internal const string RoleIsNotAssignable = "User.RoleIsNotAssignable";
    internal const string AlreadyExists = "User.AlreadyExists";
    internal const string NotFound = "User.NotFound";
    internal const string CannotDeleteSelf = "User.CannotDeleteSelf";
    internal const string CannotDeleteSupervisor = "User.CannotDeleteSupervisor";
    internal const string DirectoryUnavailable = "User.DirectoryUnavailable";
    internal const string CreationNotRecorded = "User.CreationNotRecorded";
}

internal static class UserErrorMessages
{
    internal const string EmailCannotBeEmpty = "Email cannot be empty.";
    internal const string EmailIsInvalid = "Email is not a valid email address.";
    internal const string DisplayNameCannotBeEmpty = "Display name cannot be empty.";
    internal const string DisplayNameTooLong = "Display name cannot be longer than 100 characters.";
    internal const string RoleIsNotAssignable = "This role cannot be assigned from the application.";
    internal const string AlreadyExists = "A user with this email already exists.";
    internal const string NotFound = "User doesn't exist.";
    internal const string CannotDeleteSelf = "You cannot delete your own account.";
    internal const string CannotDeleteSupervisor = "A Supervisor cannot be deleted from the application.";
    internal const string DirectoryUnavailable = "The user directory could not be reached. Please try again later.";
    internal const string CreationNotRecorded = "The user could not be created. Please try again later.";
}
