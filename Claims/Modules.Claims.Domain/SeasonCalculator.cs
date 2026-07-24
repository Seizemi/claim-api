using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Domain;

public static class SeasonCalculator
{
    public static (string SeasonValue, string SeasonLabel) Compute(DateTimeOffset dateOfArrival)
    {
        bool isSummer = dateOfArrival.Month is >= 5 and <= 10;

        if (isSummer)
        {
            var year = dateOfArrival.Year;
            return ($"ete{year}", $"Été {year}");
        }

        var startYear = dateOfArrival.Month is 11 or 12 ? dateOfArrival.Year : dateOfArrival.Year - 1;
        var endYear = startYear + 1;
        return ($"hiver{startYear}-{endYear}", $"Hiver {startYear}-{endYear}");
    }
}
