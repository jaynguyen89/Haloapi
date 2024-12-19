namespace HelperLibrary.Shared.Helpers; 

public static class DateTimeHelpers {

    public static DateTime? ToDateTime(this string any, bool exact = true) {
        if (exact) return DateTime.ParseExact(any, "dd/MM/yyyy, HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        
        var success = DateTime.TryParse(any, out var result);
        return success ? result : null;
    }

    public static double? GetAge(this DateTime? dateTime, int minAge = 15) {
        if (dateTime is null || dateTime > DateTime.UtcNow) return null;
        var totalDays = (DateTime.UtcNow - dateTime).Value.TotalDays;

        var age = totalDays / Constants.DaysPerYear;
        return age > minAge ? Math.Round(age, 1) : null;
    }

    public static long ToTimestamp(this DateTime any) => ((DateTimeOffset)any).ToUnixTimeMilliseconds();

    public static DateTime Compute(this DateTime dateTime, int value, Enums.TimeUnit unit, bool addition = true) {
        var valueInMilliseconds = value.ToMilliseconds(unit);
        return dateTime.AddMilliseconds(addition ? valueInMilliseconds : valueInMilliseconds * -1);
    }

    public static long ToMilliseconds(this int value, Enums.TimeUnit unit) {
        if (unit == Enums.TimeUnit.Millisecond) return value;
        if (unit == Enums.TimeUnit.Second) return value * Constants.TicksPerSecond;

        var timeSpan = unit switch {
            Enums.TimeUnit.Hour => TimeSpan.FromHours(value),
            Enums.TimeUnit.Day => TimeSpan.FromDays(value),
            Enums.TimeUnit.Week => TimeSpan.FromDays(value * Constants.DaysPerWeek),
            Enums.TimeUnit.Month => TimeSpan.FromDays(value * Constants.DaysPerMonth),
            Enums.TimeUnit.Year => TimeSpan.FromDays(value * (int)Constants.DaysPerYear),
            _ => TimeSpan.FromMinutes(value), // case Enums.TimeUnit.Minute
        };

        return (long)timeSpan.TotalMilliseconds;
    }

    public static string Format(this DateTime dateTime, Enums.DateFormat? dateFormat, Enums.TimeFormat? timeFormat) {
        if (dateFormat is null && timeFormat is null)
            throw new ParamsNullException("Unable to format DateTime object: both Date and Time formats are null.");
        
        if (dateFormat is not null && timeFormat is not null)
            return dateTime.ToString($"{dateFormat.GetValue()}, {timeFormat.GetValue()}");

        return dateFormat is null
            ? dateTime.ToString($"{timeFormat!.Value.GetValue()}")
            : dateTime.ToString($"{dateFormat!.Value.GetValue()}");
    }
}