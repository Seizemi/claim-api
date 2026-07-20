using FluentValidation.TestHelper;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Validators;
using Modules.Claims.Features.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.Shared.Validators;

public sealed class ClaimRequestValidatorTests
{
    private readonly ClaimRequestValidator _validator = new();

    [Fact]
    public void Validate_WithNullBooking_HasValidationErrorForBooking()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest() with { Booking = null! };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Booking)
            .WithErrorCode(BookingErrorCodes.BookingCannotBeNull);
    }

    [Fact]
    public void Validate_WithBookingNumberEmpty_HasValidationErrorForNestedBookingNumber()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        request = request with { Booking = request.Booking with { BookingNumber = string.Empty } };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("Booking.BookingNumber")
            .WithErrorCode(ClaimErrorCodes.ClaimBookingNumberCannotBeNullOrEmpty);
    }

    [Fact]
    public void Validate_WithNullCustomer_HasValidationErrorForNestedCustomer()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        request = request with { Booking = request.Booking with { Customer = null! } };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("Booking.Customer")
            .WithErrorCode(CustomerErrorCodes.CustomerCannotBeNull);
    }

    [Fact]
    public void Validate_WithCustomerNameEmpty_HasValidationErrorForNestedCustomerName()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        request = request with
        {
            Booking = request.Booking with
            {
                Customer = request.Booking.Customer with { Name = string.Empty }
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("Booking.Customer.Name")
            .WithErrorCode(ClaimErrorCodes.ClaimCustomerNameCannotBeNullOrEmpty);
    }

    [Fact]
    public void Validate_WithNullSupplier_HasValidationErrorForNestedSupplier()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        request = request with { Booking = request.Booking with { Supplier = null! } };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("Booking.Supplier")
            .WithErrorCode(SupplierErrorCodes.SupplierCannotBeNull);
    }

    [Fact]
    public void Validate_WithSupplierNameEmpty_HasValidationErrorForNestedSupplierName()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        request = request with
        {
            Booking = request.Booking with
            {
                Supplier = request.Booking.Supplier with { Name = string.Empty }
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("Booking.Supplier.Name")
            .WithErrorCode(ClaimErrorCodes.ClaimSupplierNameCannotBeNullOrEmpty);
    }

    [Fact]
    public void Validate_WithDepartureDateBeforeArrivalDate_HasValidationErrorForClaimDate()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        request = request with
        {
            ClaimDate = request.ClaimDate with
            {
                DateOfDeparture = DateTimeOffset.UtcNow.AddDays(-1),
                DateOfArrival = DateTimeOffset.UtcNow
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("ClaimDate")
            .WithErrorCode(ClaimErrorCodes.ClaimDateOfDepartureCannotBeSmallerThanDateOfArrival);
    }

    [Fact]
    public void Validate_WithNullClaimDate_HasNoValidationErrorForClaimDate()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest() with { ClaimDate = null! };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ClaimDate);
    }

    [Fact]
    public void Validate_WithValidRequest_HasNoValidationErrors()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
