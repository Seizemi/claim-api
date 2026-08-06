using System.Text.RegularExpressions;
using Modules.Claims.Domain.Enums;

namespace Modules.Claims.Domain;

public static partial class SeasonCalculator
{
    public static (string SeasonValue, string SeasonLabel) Compute(DateOnly dateOfArrival)
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

    public static bool TryResolveDateRange(string seasonValue, out DateOnly startDate, out DateOnly endDate)
    {
        startDate = default;
        endDate = default;

        var summerMatch = SummerPattern().Match(seasonValue);
        if (summerMatch.Success)
        {
            var year = int.Parse(summerMatch.Groups["year"].Value);
            startDate = new DateOnly(year, 5, 1);
            endDate = new DateOnly(year, 10, 31);
            return true;
        }

        var winterMatch = WinterPattern().Match(seasonValue);
        if (winterMatch.Success)
        {
            var startYear = int.Parse(winterMatch.Groups["start"].Value);
            var endYear = int.Parse(winterMatch.Groups["end"].Value);
            if (endYear != startYear + 1)
            {
                return false;
            }

            startDate = new DateOnly(startYear, 11, 1);
            endDate = new DateOnly(endYear, 4, 30);
            return true;
        }

        return false;
    }

    [GeneratedRegex(@"^ete(?<year>\d{4})$")]
    private static partial Regex SummerPattern();

    [GeneratedRegex(@"^hiver(?<start>\d{4})-(?<end>\d{4})$")]
    private static partial Regex WinterPattern();
}
