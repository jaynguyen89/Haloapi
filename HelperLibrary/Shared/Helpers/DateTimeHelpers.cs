namespace HelperLibrary.Shared.Helpers; 

public static class DateTimeHelpers {
    
    public static int? GetAge(this DateTime? dateTime, int minAge = 15) {
        if (dateTime is null || dateTime > DateTime.UtcNow) return default;

        var age = DateTime.UtcNow.Year - dateTime.Value.Year;
        if (dateTime > DateTime.UtcNow.AddYears(age)) age--;
        return age > minAge ? age : default;
    }

    public static long ToTimestamp(this DateTime any) => ((DateTimeOffset)any).ToUnixTimeMilliseconds();

    public static DateTime Compute(this DateTime dateTime, int value, Enums.TimeUnit unit, bool addition = true) {
        var valueInMilliseconds = ToMilliseconds(value, unit);
        return dateTime.AddMilliseconds(addition ? valueInMilliseconds : valueInMilliseconds * -1);
    }

    public static long ToMilliseconds(this int value, Enums.TimeUnit unit) => unit switch {
        Enums.TimeUnit.Second => value * Constants.TicksPerSecond,
        Enums.TimeUnit.Minute => ToMilliseconds(value * Constants.SecondsPerMinute, Enums.TimeUnit.Second),
        Enums.TimeUnit.Hour => ToMilliseconds(value * Constants.MinutesPerHour, Enums.TimeUnit.Minute),
        Enums.TimeUnit.Day => ToMilliseconds(value * Constants.HoursPerDay, Enums.TimeUnit.Hour),
        Enums.TimeUnit.Week => ToMilliseconds(value * Constants.DaysPerWeek, Enums.TimeUnit.Day),
        Enums.TimeUnit.Month => ToMilliseconds(value * Constants.DaysPerMonth, Enums.TimeUnit.Day),
        Enums.TimeUnit.Quarter => ToMilliseconds(value * Constants.MonthsPerQuarter, Enums.TimeUnit.Month),
        Enums.TimeUnit.Year => ToMilliseconds(value * Constants.DaysPerYear, Enums.TimeUnit.Day),
        _ => value,
    };

    public static string Format(this DateTime dateTime, Enums.DateFormat dateFormat, Enums.TimeFormat timeFormat) =>
        dateTime.ToString($"{dateFormat.GetValue()} {timeFormat.GetValue()}");
}