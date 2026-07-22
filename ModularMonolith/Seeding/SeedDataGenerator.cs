using Modules.Claims.Domain.Entities;
using Modules.Claims.Domain.Enums;

namespace ModularMonolith.Seeding;

internal static class SeedDataGenerator
{
    private static readonly string[] CompanyWords =
    [
        "Alpine", "Summit", "Glacier", "Horizon", "Northwind", "Bluewave", "Cedar", "Skyline",
        "Meridian", "Coastal", "Highland", "Vista", "Frontier", "Pinnacle", "Sunrise", "Evergreen"
    ];

    private static readonly string[] CompanySuffixes = ["Travel", "Tours", "Voyages", "Holidays", "Escapes", "Trips"];

    private static readonly string[] FirstNames =
    [
        "Emma", "Liam", "Olivia", "Noah", "Ava", "Lucas", "Mia", "Hugo", "Chloe", "Leo",
        "Camille", "Louis", "Sarah", "Jules", "Manon", "Nathan", "Lea", "Adam", "Ines", "Paul"
    ];

    private static readonly string[] LastNames =
    [
        "Martin", "Bernard", "Dubois", "Thomas", "Robert", "Petit", "Durand", "Leroy",
        "Moreau", "Simon", "Laurent", "Lefebvre", "Michel", "Garcia", "David", "Bertrand"
    ];

    private static readonly string[] Products = ["Ski Package", "Chalet Stay", "Resort Combo", "Family Deal", "Group Trip", "Weekend Getaway"];

    private static readonly string?[] ReasonPool =
    [
        "Customer arrived to find the accommodation did not match the booking description.",
        "Flight was cancelled and no alternative was offered by the supplier.",
        "Equipment rental was unavailable on arrival despite confirmation.",
        "Booking was double-charged on the customer's card.",
        "Customer had to cancel due to a medical emergency.",
        null
    ];

    private static readonly string?[] ClaimSummaryPool =
    [
        "Awaiting supplier confirmation on refund eligibility.",
        "Customer has been offered a partial voucher as compensation.",
        "Escalated to supplier for further investigation.",
        "Resolved after supplier agreed to reimburse the customer.",
        null
    ];

    private static readonly string?[] PurposeOfSolutionPool =
    [
        "Goodwill gesture to retain customer loyalty.",
        "Supplier failed to honor the original booking terms.",
        "Compensation for the inconvenience caused during the trip.",
        null
    ];

    internal static List<Supplier> GenerateSuppliers(int count, Random random) =>
        Enumerable.Range(0, count)
            .Select(_ => new Supplier
            {
                Id = Guid.CreateVersion7(),
                Name = $"{PickRandom(CompanyWords, random)} {PickRandom(CompanySuffixes, random)}",
                SupplierAkioNumber = random.Next(10000, 99999)
            })
            .ToList();

    internal static Claim GenerateClaim(Supplier supplier, Random random)
    {
        var claimId = Guid.CreateVersion7();
        var bookingId = Guid.CreateVersion7();

        return new Claim
        {
            Id = claimId,
            State = RandomEnum<ClaimState>(random),
            FollowedBy = random.Next(2) == 0 ? RandomFullName(random) : null,
            Reason = PickRandom(ReasonPool, random),
            ClaimSummary = PickRandom(ClaimSummaryPool, random),
            Solution = random.Next(2) == 0 ? RandomEnum<ClaimSolution>(random) : null,
            PurposeOfSolution = PickRandom(PurposeOfSolutionPool, random),
            UpdateReason = null,
            CustomerSuppInfo = random.Next(3) == 0 ? "Customer provided additional documentation." : null,
            SupplierSuppInfo = random.Next(3) == 0 ? "Supplier acknowledged receipt of the claim." : null,
            BookingId = bookingId,
            Booking = GenerateBooking(bookingId, supplier, random),
            ClaimDate = GenerateClaimDate(claimId, random),
            Compensation = GenerateCompensation(claimId, random)
        };
    }

    private static Booking GenerateBooking(Guid bookingId, Supplier supplier, Random random)
    {
        var customerId = Guid.CreateVersion7();
        var seasonYear = random.Next(2023, 2027);

        return new Booking
        {
            Id = bookingId,
            BookingNumber = $"BK{random.Next(100000, 999999)}",
            SalesChannel = RandomEnum<SalesChannel>(random),
            Language = RandomEnum<Language>(random),
            SeasonLabel = $"Winter {seasonYear}/{seasonYear + 1}",
            SeasonValue = $"{seasonYear}-{seasonYear + 1}",
            Service = RandomEnum<BookingService>(random),
            Skissim = random.Next(2) == 0,
            SkissimType = RandomEnum<SkissimType>(random),
            Product = PickRandom(Products, random),
            CustomerId = customerId,
            Customer = new Customer
            {
                Id = customerId,
                Name = RandomFullName(random),
                AkioNumber = random.Next(100000, 999999)
            },
            SupplierId = supplier.Id,
            Supplier = supplier
        };
    }

    private static ClaimDate GenerateClaimDate(Guid claimId, Random random)
    {
        var dateOfReceivedClaim = DateTimeOffset.UtcNow.AddDays(-random.Next(0, 730));
        var dateOfArrival = DateTimeOffset.UtcNow.AddDays(random.Next(-365, 365));
        var dateOfDeparture = dateOfArrival.AddDays(random.Next(1, 21));

        return new ClaimDate
        {
            Id = Guid.CreateVersion7(),
            ClaimId = claimId,
            DateOfReceivedClaim = dateOfReceivedClaim,
            DateOfStartFollowUp = dateOfReceivedClaim.AddDays(random.Next(0, 5)),
            DateLastUpdate = dateOfReceivedClaim.AddDays(random.Next(0, 30)),
            DateOfDeparture = dateOfDeparture,
            DateEndOfFollowUp = random.Next(2) == 0 ? dateOfReceivedClaim.AddDays(random.Next(5, 60)) : null,
            DateOfArrival = dateOfArrival
        };
    }

    private static Compensation GenerateCompensation(Guid claimId, Random random) => new()
    {
        Id = Guid.CreateVersion7(),
        ClaimId = claimId,
        CustomerVoucher = random.Next(2) == 0 ? (float)random.Next(0, 500) : null,
        CustomerUsedVoucher = random.Next(2) == 0 ? (float)random.Next(0, 500) : null,
        SupplierRefund = random.Next(2) == 0 ? (float)random.Next(0, 1000) : null,
        ClaimRefund = random.Next(2) == 0 ? (float)random.Next(0, 1000) : null,
        RefundState = RandomEnum<RefundState>(random)
    };

    private static string RandomFullName(Random random) => $"{PickRandom(FirstNames, random)} {PickRandom(LastNames, random)}";

    private static T PickRandom<T>(T[] pool, Random random) => pool[random.Next(pool.Length)];

    private static TEnum RandomEnum<TEnum>(Random random) where TEnum : struct, Enum
    {
        var values = Enum.GetValues<TEnum>();
        return values[random.Next(values.Length)];
    }
}
