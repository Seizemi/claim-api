using FluentValidation.TestHelper;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Validators;
using Modules.Claims.Features.Tests.Shared;

namespace Modules.Claims.Features.Tests.Features.Shared.Validators;

[TestClass]
public sealed class ClaimRequestValidatorTests
{
    private readonly ClaimRequestValidator _validator = new();

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
    public void Validate_WithDepartureDateAfterArrivalDate_HasValidationErrorForClaimDate()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        request = request with
        {
            ClaimDate = request.ClaimDate with
            {
                DateOfDeparture = DateTimeOffset.UtcNow,
                DateOfArrival = DateTimeOffset.UtcNow.AddDays(-1)
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("ClaimDate")
            .WithErrorCode(ClaimErrorCodes.ClaimDateOfDepartureCannotBeSmallerThanDateOfArrival);
    }

    [TestMethod]
    public void Validate_WithNullClaimDate_HasNoValidationErrorForClaimDate()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest() with { ClaimDate = null! };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ClaimDate);
    }

    [TestMethod]
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
