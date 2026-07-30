using FluentValidation;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;

namespace Modules.Claims.Features.Features.Shared.Validators;

internal sealed class ClaimRequestValidator : AbstractValidator<ClaimRequest>
{
    public ClaimRequestValidator()
    {
        RuleFor(x => x.ReasonId)
            .NotEmpty()
            .WithErrorCode(ClaimErrorCodes.ClaimReasonIdCannotBeEmpty)
            .WithMessage(ClaimErrorMessages.ClaimReasonIdCannotBeEmpty);

        RuleFor(x => x.SolutionId)
            .NotEmpty()
            .WithErrorCode(ClaimErrorCodes.ClaimSolutionIdCannotBeEmpty)
            .WithMessage(ClaimErrorMessages.ClaimSolutionIdCannotBeEmpty);

        RuleFor(x => x.Booking)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode(BookingErrorCodes.BookingCannotBeNull)
            .WithMessage(BookingErrorMessages.BookingCannotBeNull)
            .SetValidator(new BookingRequestValidator());

        RuleFor(x => x.ClaimDate)
            .SetValidator(new ClaimDateRequestValidator())
            .When(x => x.ClaimDate is not null);

        RuleFor(x => x.Compensation)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode(CompensationErrorCodes.CompensationCannotBeNull)
            .WithMessage(CompensationErrorMessages.CompensationCannotBeNull)
            .SetValidator(new CompensationRequestValidator());
    }
}

internal sealed class BookingRequestValidator : AbstractValidator<BookingRequest>
{
    public BookingRequestValidator()
    {
        RuleFor(x => x.BookingNumber)
            .NotEmpty()
            .WithErrorCode(ClaimErrorCodes.ClaimBookingNumberCannotBeNullOrEmpty)
            .WithMessage(ClaimErrorMessages.ClaimBookingNumberCannotBeNullOrEmpty);

        RuleFor(x => x.SalesChannelId)
            .NotEmpty()
            .WithErrorCode(ClaimErrorCodes.ClaimSalesChannelIdCannotBeEmpty)
            .WithMessage(ClaimErrorMessages.ClaimSalesChannelIdCannotBeEmpty);

        RuleFor(x => x.SkissimTypeId)
            .NotEmpty()
            .WithErrorCode(ClaimErrorCodes.ClaimSkissimTypeIdCannotBeEmpty)
            .WithMessage(ClaimErrorMessages.ClaimSkissimTypeIdCannotBeEmpty);

        RuleFor(x => x.Customer)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode(CustomerErrorCodes.CustomerCannotBeNull)
            .WithMessage(CustomerErrorMessages.CustomerCannotBeNull)
            .SetValidator(new CustomerRequestValidator());

        RuleFor(x => x.Supplier)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode(SupplierErrorCodes.SupplierCannotBeNull)
            .WithMessage(SupplierErrorMessages.SupplierCannotBeNull)
            .SetValidator(new SupplierRequestValidator());
    }
}

internal sealed class CustomerRequestValidator : AbstractValidator<CustomerRequest>
{
    public CustomerRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(ClaimErrorCodes.ClaimCustomerNameCannotBeNullOrEmpty)
            .WithMessage(ClaimErrorMessages.ClaimCustomerNameCannotBeNullOrEmpty);
    }
}

internal sealed class SupplierRequestValidator : AbstractValidator<SupplierRequest>
{
    public SupplierRequestValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty()
            .WithErrorCode(ClaimErrorCodes.ClaimSupplierLabelCannotBeNullOrEmpty)
            .WithMessage(ClaimErrorMessages.ClaimSupplierLabelCannotBeNullOrEmpty);

        RuleFor(x => x.Value)
            .NotEmpty()
            .WithErrorCode(ClaimErrorCodes.ClaimSupplierValueCannotBeNullOrEmpty)
            .WithMessage(ClaimErrorMessages.ClaimSupplierValueCannotBeNullOrEmpty);

        RuleFor(x => x.ServiceId)
            .NotEmpty()
            .WithErrorCode(ClaimErrorCodes.ClaimSupplierServiceIdCannotBeEmpty)
            .WithMessage(ClaimErrorMessages.ClaimSupplierServiceIdCannotBeEmpty);
    }
}

internal sealed class ClaimDateRequestValidator : AbstractValidator<ClaimDateRequest>
{
    public ClaimDateRequestValidator()
    {
        RuleFor(x => x.DateOfArrival)
            .NotNull()
            .WithErrorCode(ClaimErrorCodes.ClaimDateOfArrivalCannotBeNull)
            .WithMessage(ClaimErrorMessages.ClaimDateOfArrivalCannotBeNull);

        RuleFor(x => x)
            .Must(cd => cd.DateOfDeparture is null
                     || cd.DateOfArrival is null
                     || cd.DateOfDeparture >= cd.DateOfArrival)
            .WithErrorCode(ClaimErrorCodes.ClaimDateOfDepartureCannotBeSmallerThanDateOfArrival)
            .WithMessage(ClaimErrorMessages.ClaimDateOfDepartureCannotBeSmallerThanDateOfArrival);
    }
}

internal sealed class CompensationRequestValidator : AbstractValidator<CompensationRequest>
{
    public CompensationRequestValidator()
    {
        RuleFor(x => x.RefundStateId)
            .NotEmpty()
            .WithErrorCode(ClaimErrorCodes.ClaimRefundStateIdCannotBeEmpty)
            .WithMessage(ClaimErrorMessages.ClaimRefundStateIdCannotBeEmpty);

        RuleFor(x => x.CompensationReasonId)
            .NotEmpty()
            .WithErrorCode(ClaimErrorCodes.ClaimCompensationReasonIdCannotBeEmpty)
            .WithMessage(ClaimErrorMessages.ClaimCompensationReasonIdCannotBeEmpty);
    }
}
