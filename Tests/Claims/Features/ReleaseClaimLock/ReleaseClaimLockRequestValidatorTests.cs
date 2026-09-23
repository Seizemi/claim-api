using FluentValidation.TestHelper;
using Modules.Claims.Features.Features.ReleaseClaimLock;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.ReleaseClaimLock;

public sealed class ReleaseClaimLockRequestValidatorTests
{
    private readonly ReleaseClaimLockRequestValidator _validator = new();

    [Fact]
    public void Validate_WithEmptyUserId_HasValidationErrorForUserId()
    {
        // Arrange
        var request = new ReleaseClaimLockRequest(Guid.Empty);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorCode(ClaimErrorCodes.ClaimLockUserIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WithNonEmptyUserId_HasNoValidationErrors()
    {
        // Arrange
        var request = new ReleaseClaimLockRequest(Guid.CreateVersion7());

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
