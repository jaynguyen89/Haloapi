using HelperLibrary.Shared;

namespace HelperLibrary; 

public static class DateTimeHelpers {
    
    public static int? GetAge(this DateTime? dateTime) {
        if (dateTime is null) return default;

        var age = DateTime.UtcNow.Year - dateTime.Value.Year;
        if (dateTime > DateTime.UtcNow.AddYears(age)) age--;
        return age;
    }

    public static DateTime? Compute(this DateTime dateTime, int value, Enums.TimeUnit unit) {
        var valueInMilliseconds = ToMilliseconds(value, unit);
        return dateTime.AddMilliseconds(valueInMilliseconds);
    }

    public static long ToMilliseconds(this int value, Enums.TimeUnit unit) => unit switch {
        Enums.TimeUnit.SECOND => value * Constants.TicksPerSecond,
        Enums.TimeUnit.MINUTE => ToMilliseconds(value * Constants.SecondsPerMinute, Enums.TimeUnit.SECOND),
        Enums.TimeUnit.HOUR => ToMilliseconds(value * Constants.MinutesPerHour, Enums.TimeUnit.MINUTE),
        Enums.TimeUnit.DAY => ToMilliseconds(value * Constants.HoursPerDay, Enums.TimeUnit.HOUR),
        Enums.TimeUnit.WEEK => ToMilliseconds(value * Constants.DaysPerWeek, Enums.TimeUnit.DAY),
        Enums.TimeUnit.MONTH => ToMilliseconds(value * Constants.DaysPerMonth, Enums.TimeUnit.DAY),
        Enums.TimeUnit.QUARTER => ToMilliseconds(value * Constants.MonthsPerQuarter, Enums.TimeUnit.MONTH),
        Enums.TimeUnit.YEAR => ToMilliseconds(value * Constants.DaysPerYear, Enums.TimeUnit.DAY),
        _ => value
    };

    public static string? Format(this DateTime? dateTime, Enums.DateFormat dateFormat, Enums.TimeFormat timeFormat) =>
        dateTime?.ToString($"{dateFormat.GetEnumValue<string>()} {timeFormat.GetEnumValue<string>()}");
}