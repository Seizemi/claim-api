using FluentValidation.TestHelper;
using Modules.Claims.Features.Features.AcquireClaimLock;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.AcquireClaimLock;

public sealed class AcquireClaimLockRequestValidatorTests
{
    private readonly AcquireClaimLockRequestValidator _validator = new();

    [Fact]
    public void Validate_WithEmptyUserId_HasValidationErrorForUserId()
    {
        // Arrange
        var request = new AcquireClaimLockRequest(Guid.Empty, "Alice");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorCode(ClaimErrorCodes.ClaimLockUserIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WithEmptyUserName_HasValidationErrorForUserName()
    {
        // Arrange
        var request = new AcquireClaimLockRequest(Guid.CreateVersion7(), string.Empty);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorCode(ClaimErrorCodes.ClaimLockUserNameCannotBeNullOrEmpty);
    }

    [Fact]
    public void Validate_WithValidRequest_HasNoValidationErrors()
    {
        // Arrange
        var request = new AcquireClaimLockRequest(Guid.CreateVersion7(), "Alice");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
