using FluentValidation.TestHelper;
using Modules.Claims.Features.Features.GetClaimsBySeason;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.GetClaimsBySeason;

public sealed class GetClaimsBySeasonRequestValidatorTests
{
    private readonly GetClaimsBySeasonRequestValidator _validator = new();

    [Theory]
    [InlineData("not-a-season")]
    [InlineData("ete20255")]
    [InlineData("hiver2025-2028")]
    [InlineData("")]
    public void Validate_WithInvalidSeasonValue_HasValidationErrorForSeasonValue(string seasonValue)
    {
        // Arrange
        var request = new GetClaimsBySeasonRequest(seasonValue);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SeasonValue)
            .WithErrorCode(SeasonErrorCodes.SeasonValueIsInvalid);
    }

    [Theory]
    [InlineData("ete2025")]
    [InlineData("hiver2025-2026")]
    public void Validate_WithValidSeasonValue_HasNoValidationErrors(string seasonValue)
    {
        // Arrange
        var request = new GetClaimsBySeasonRequest(seasonValue);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
