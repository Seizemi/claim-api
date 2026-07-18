using Bogus;
using Modules.Claims.Domain.Entities;
using Modules.Claims.Domain.Enums;
using Modules.Claims.Features.Features.Shared.Requests;

namespace Modules.Claims.Features.Tests.Shared;

internal static class ClaimTestDataFactory
{
    internal static Claim CreateClaim(DateTimeOffset dateOfReceivedClaim)
    {
        var faker = new Faker();
        var claimId = Guid.CreateVersion7();
        var bookingId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var supplierId = Guid.CreateVersion7();

        return new Claim
        {
            Id = claimId,
            State = faker.PickRandom<ClaimState>(),
            BookingId = bookingId,
            Booking = new Booking
            {
                Id = bookingId,
                BookingNumber = faker.Random.Replace("BK####"),
                SeasonLabel = faker.Commerce.Department(),
                SeasonValue = faker.Random.Word(),
                CustomerId = customerId,
                Customer = new Customer
                {
                    Id = customerId,
                    Name = faker.Person.FullName,
                    AkioNumber = faker.Random.Int(1000, 9999)
                },
                SupplierId = supplierId,
                Supplier = new Supplier
                {
                    Id = supplierId,
                    Name = faker.Company.CompanyName(),
                    SupplierAkioNumber = faker.Random.Int(1000, 9999)
                }
            },
            ClaimDate = new ClaimDate
            {
                Id = Guid.CreateVersion7(),
                ClaimId = claimId,
                DateOfReceivedClaim = dateOfReceivedClaim
            },
            Compensation = new Compensation
            {
                Id = Guid.CreateVersion7(),
                ClaimId = claimId
            }
        };
    }

    internal static ClaimRequest CreateClaimRequest()
    {
        var faker = new Faker();

        return new ClaimRequest(
            State: faker.PickRandom<ClaimState>(),
            FollowedBy: faker.Person.FullName,
            Reason: faker.Lorem.Sentence(),
            ClaimSummary: faker.Lorem.Paragraph(),
            Solution: faker.PickRandom<ClaimSolution>(),
            PurposeOfSolution: faker.Lorem.Sentence(),
            UpdateReason: null,
            CustomerSuppInfo: faker.Lorem.Sentence(),
            SupplierSuppInfo: faker.Lorem.Sentence(),
            Booking: new BookingRequest(
                BookingNumber: faker.Random.Replace("BK####"),
                SalesChannel: faker.PickRandom<SalesChannel>(),
                Language: faker.PickRandom<Language>(),
                SeasonLabel: faker.Commerce.Department(),
                SeasonValue: faker.Random.Word(),
                Service: faker.PickRandom<BookingService>(),
                Skissim: faker.Random.Bool(),
                SkissimType: faker.PickRandom<SkissimType>(),
                Product: faker.Commerce.ProductName(),
                Customer: new CustomerRequest(
                    Name: faker.Person.FullName,
                    AkioNumber: faker.Random.Int(1000, 9999)),
                Supplier: new SupplierRequest(
                    Name: faker.Company.CompanyName(),
                    SupplierAkioNumber: faker.Random.Int(1000, 9999))),
            ClaimDate: new ClaimDateRequest(
                DateOfReceivedClaim: DateTimeOffset.UtcNow,
                DateOfStartFollowUp: null,
                DateLastUpdate: null,
                DateOfDeparture: null,
                DateEndOfFollowUp: null,
                DateOfArrival: null),
            Compensation: new CompensationRequest(
                CustomerVoucher: faker.Random.Float(0, 500),
                CustomerUsedVoucher: null,
                SupplierRefund: null,
                ClaimRefund: null,
                RefundState: faker.PickRandom<RefundState>()));
    }
}
