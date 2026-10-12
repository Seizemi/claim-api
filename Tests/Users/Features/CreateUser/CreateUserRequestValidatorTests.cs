using Microsoft.Extensions.Options;
using Modules.Users.Features.Configuration;
using Modules.Users.Features.Features.CreateUser;
using Modules.Users.Features.Features.Shared.Errors;
using Modules.Users.Features.Features.Shared.Requests;
using Xunit;

namespace Modules.Users.Features.Tests.Features.CreateUser;

public sealed class CreateUserRequestValidatorTests
{
    private static readonly CreateUserRequest ValidRequest = new("agent@example.com", "Ann Agent", "Agent");

    private readonly CreateUserRequestValidator _validator =
        new(Options.Create(new UserManagementSettings { AssignableRoles = ["Agent"] }));

    [Fact]
    public void Validate_WithValidRequest_Passes()
    {
        var result = _validator.Validate(ValidRequest);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("first.last+tag@sub.example.com")]
    [InlineData("o'brien@example.co.uk")]
    public void Validate_WithValidEmail_Passes(string email)
    {
        var result = _validator.Validate(ValidRequest with { Email = email });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyEmail_FailsWithEmptyCode(string email)
    {
        var result = _validator.Validate(ValidRequest with { Email = email });

        Assert.Contains(result.Errors, error => error.ErrorCode == UserErrorCodes.EmailCannotBeEmpty);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("agent@localhost")]
    [InlineData("Ann <agent@example.com>")]
    [InlineData(" agent@example.com")]
    [InlineData("agent@example.com;other@example.com")]
    public void Validate_WithInvalidEmail_FailsWithInvalidCode(string email)
    {
        var result = _validator.Validate(ValidRequest with { Email = email });

        var error = Assert.Single(result.Errors);
        Assert.Equal(UserErrorCodes.EmailIsInvalid, error.ErrorCode);
    }

    [Fact]
    public void Validate_WithEmailLongerThan254_Fails()
    {
        var result = _validator.Validate(ValidRequest with { Email = new string('a', 243) + "@example.com" });

        Assert.Contains(result.Errors, error => error.ErrorCode == UserErrorCodes.EmailIsInvalid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_WithEmptyDisplayName_Fails(string displayName)
    {
        var result = _validator.Validate(ValidRequest with { DisplayName = displayName });

        Assert.Contains(result.Errors, error => error.ErrorCode == UserErrorCodes.DisplayNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WithDisplayNameOf100Characters_Passes()
    {
        var result = _validator.Validate(ValidRequest with { DisplayName = new string('a', CreateUserRequestValidator.DisplayNameMaxLength) });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithDisplayNameOver100Characters_Fails()
    {
        var result = _validator.Validate(ValidRequest with { DisplayName = new string('a', CreateUserRequestValidator.DisplayNameMaxLength + 1) });

        Assert.Contains(result.Errors, error => error.ErrorCode == UserErrorCodes.DisplayNameTooLong);
    }

    [Theory]
    [InlineData("Supervisor")]
    [InlineData("agent")]
    [InlineData("")]
    [InlineData("Admin")]
    public void Validate_WithNonAssignableRole_Fails(string role)
    {
        var result = _validator.Validate(ValidRequest with { Role = role });

        var error = Assert.Single(result.Errors);
        Assert.Equal(UserErrorCodes.RoleIsNotAssignable, error.ErrorCode);
    }
}
