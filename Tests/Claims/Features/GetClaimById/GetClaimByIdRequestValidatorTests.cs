using FluentValidation.TestHelper;
using Modules.Claims.Features.Features.GetClaimById;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.GetClaimById;

public sealed class GetClaimByIdRequestValidatorTests
{
    private readonly GetClaimByIdRequestValidator _validator = new();

    [Fact]
    public void Validate_WithEmptyClaimId_HasValidationErrorForClaimId()
    {
        // Arrange
        var request = new GetClaimByIdRequest(Guid.Empty);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClaimId)
            .WithErrorCode(ClaimErrorCodes.ClaimIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WithNonEmptyClaimId_HasNoValidationErrors()
    {
        // Arrange
        var request = new GetClaimByIdRequest(Guid.CreateVersion7());

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
