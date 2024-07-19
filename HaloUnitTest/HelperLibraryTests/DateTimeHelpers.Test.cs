using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;

namespace HaloUnitTest.HelperLibraryTests;

[TestFixture]
public sealed class DateTimeHelpersTest {

    [Test]
    public void Test_GetAge() {
        // Test for null input
        DateTime? birthday = null;
        var result = birthday.GetAge();
        Assert.That(result, Is.Null);

        // Test for a future date input
        birthday = DateTime.UtcNow.AddDays(1);
        result = birthday.GetAge();
        Assert.That(result, Is.Null);
        
        // Test for unsatisfied min age
        const int minAge = 2;
        var birthdays = new[] {
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddMonths(-1),
            DateTime.UtcNow.AddYears(-1),
        };
        var results = birthdays.Select(x => ((DateTime?)x).GetAge(minAge)).ToList();
        results.ForEach(res => Assert.That(res, Is.Null));

        const int expect = 16;
        result = ((DateTime?)DateTime.UtcNow.AddYears(-1 * expect)).GetAge();
        Assert.That((int)result!, Is.EqualTo(expect));
    }

    [Test]
    public void Test_ToTimestamp() {
        var dates = new[] {
            new DateTime(1932, 9, 16),
            new DateTime(2136, 4, 22, 1, 39, 44),
        };

        var results = dates.Select(date => date.ToTimestamp()).ToList();
        var expects = new[] { -1176890400000, 5248078784000 };
        
        results.ForEach(result => Assert.That(result, Is.EqualTo(expects[results.IndexOf(result)])));
    }

    [Test]
    public void Test_ToMilliseconds() {
        var durations = new[] {
            new KeyValuePair<Enums.TimeUnit, int>(Enums.TimeUnit.Millisecond, 1234567890),
            new KeyValuePair<Enums.TimeUnit, int>(Enums.TimeUnit.Second, 13257),
            new KeyValuePair<Enums.TimeUnit, int>(Enums.TimeUnit.Minute, 8901),
            new KeyValuePair<Enums.TimeUnit, int>(Enums.TimeUnit.Hour, 2399),
            new KeyValuePair<Enums.TimeUnit, int>(Enums.TimeUnit.Day, 55),
            new KeyValuePair<Enums.TimeUnit, int>(Enums.TimeUnit.Week, 11),
            new KeyValuePair<Enums.TimeUnit, int>(Enums.TimeUnit.Month, 5),
            new KeyValuePair<Enums.TimeUnit, int>(Enums.TimeUnit.Year, 2),
        };

        var results = durations.Select(duration => duration.Value.ToMilliseconds(duration.Key)).ToList();
        var expects = new[] {
            1234567890,
            13257000,
            534060000,
            8636400000,
            4752000000,
            6652800000,
            12960000000,
            63072000000,
        };
        
        results.ForEach(result => Assert.That(result, Is.EqualTo(expects[results.IndexOf(result)])));
    }

    [Test]
    public void Test_Compute() {
        var date = new DateOnly(2000, 9, 9);
        var time = new TimeOnly(9, 9, 9, 800);
        var timeSlices = new[] {
            new Tuple<DateTime, int, Enums.TimeUnit, bool>(new DateTime(date, time), 900, Enums.TimeUnit.Millisecond, true),
            new Tuple<DateTime, int, Enums.TimeUnit, bool>(new DateTime(date, time), 900, Enums.TimeUnit.Millisecond, false),
            new Tuple<DateTime, int, Enums.TimeUnit, bool>(new DateTime(date, time), 90, Enums.TimeUnit.Second, true),
            new Tuple<DateTime, int, Enums.TimeUnit, bool>(new DateTime(date, time), 90, Enums.TimeUnit.Second, false),
            new Tuple<DateTime, int, Enums.TimeUnit, bool>(new DateTime(date, time), 99, Enums.TimeUnit.Minute, true),
            new Tuple<DateTime, int, Enums.TimeUnit, bool>(new DateTime(date, time), 99, Enums.TimeUnit.Minute, false),
            new Tuple<DateTime, int, Enums.TimeUnit, bool>(new DateTime(date, time), 19, Enums.TimeUnit.Hour, true),
            new Tuple<DateTime, int, Enums.TimeUnit, bool>(new DateTime(date, time), 19, Enums.TimeUnit.Hour, false),
            new Tuple<DateTime, int, Enums.TimeUnit, bool>(new DateTime(date, time), 19, Enums.TimeUnit.Day, true),
            new Tuple<DateTime, int, Enums.TimeUnit, bool>(new DateTime(date, time), 19, Enums.TimeUnit.Day, false),
            new Tuple<DateTime, int, Enums.TimeUnit, bool>(new DateTime(date, time), 19, Enums.TimeUnit.Week, true),
            new Tuple<DateTime, int, Enums.TimeUnit, bool>(new DateTime(date, time), 19, Enums.TimeUnit.Week, false),
            new Tuple<DateTime, int, Enums.TimeUnit, bool>(new DateTime(date, time), 19, Enums.TimeUnit.Month, true),
            new Tuple<DateTime, int, Enums.TimeUnit, bool>(new DateTime(date, time), 19, Enums.TimeUnit.Month, false),
            new Tuple<DateTime, int, Enums.TimeUnit, bool>(new DateTime(date, time), 19, Enums.TimeUnit.Year, true),
            new Tuple<DateTime, int, Enums.TimeUnit, bool>(new DateTime(date, time), 19, Enums.TimeUnit.Year, false),
        };

        var results = timeSlices.Select(slice => slice.Item1.Compute(slice.Item2, slice.Item3, slice.Item4)).ToArray();
        var expects = new[] {
            new DateTime(new DateOnly(2000, 9, 9), new TimeOnly(9, 9, 10, 700)),
            new DateTime(new DateOnly(2000, 9, 9), new TimeOnly(9, 9, 8, 900)),
            new DateTime(new DateOnly(2000, 9, 9), new TimeOnly(9, 10, 39, 800)),
            new DateTime(new DateOnly(2000, 9, 9), new TimeOnly(9, 7, 39, 800)),
            new DateTime(new DateOnly(2000, 9, 9), new TimeOnly(10, 48, 9, 800)),
            new DateTime(new DateOnly(2000, 9, 9), new TimeOnly(7, 30, 9, 800)),
            new DateTime(new DateOnly(2000, 9, 10), new TimeOnly(4, 9, 9, 800)),
            new DateTime(new DateOnly(2000, 9, 8), new TimeOnly(14, 9, 9, 800)),
            new DateTime(new DateOnly(2000, 9, 28), new TimeOnly(9, 9, 9, 800)),
            new DateTime(new DateOnly(2000, 8, 21), new TimeOnly(9, 9, 9, 800)),
            new DateTime(new DateOnly(2001, 1, 20), new TimeOnly(9, 9, 9, 800)),
            new DateTime(new DateOnly(2000, 4, 29), new TimeOnly(9, 9, 9, 800)),
            new DateTime(new DateOnly(2002, 4, 2), new TimeOnly(9, 9, 9, 800)),
            new DateTime(new DateOnly(1999, 2, 17), new TimeOnly(9, 9, 9, 800)),
            new DateTime(new DateOnly(2019, 9, 5), new TimeOnly(9, 9, 9, 800)),
            new DateTime(new DateOnly(1981, 9, 14), new TimeOnly(9, 9, 9, 800)),
        };
        
        Assert.That(results, Is.EquivalentTo(expects));
    }

    [Test]
    public void Test_Format() {
        Assert.Throws<ParamsNullException>(() => new DateTime(2000, 9, 9, 9, 9, 9).Format(null, null));
        
        var datetimes = new[] {
            new DateTime(2000, 9, 9, 9, 9, 9),
            new DateTime(new DateOnly(2000, 10, 10), new TimeOnly(10, 10)),
            new DateTime(2000, 11, 11),
        };

        var formats = new[] {
            new Tuple<Enums.DateFormat?, Enums.TimeFormat?>(Enums.DateFormat.DDMMMYYYY, null),
            new Tuple<Enums.DateFormat?, Enums.TimeFormat?>(null, Enums.TimeFormat.HHMMSSC),
            new Tuple<Enums.DateFormat?, Enums.TimeFormat?>(Enums.DateFormat.WDDMMMYYYY, Enums.TimeFormat.HHMMSSTTC),
            new Tuple<Enums.DateFormat?, Enums.TimeFormat?>(Enums.DateFormat.DDMMYYYYS, Enums.TimeFormat.HHMMSSC),
            new Tuple<Enums.DateFormat?, Enums.TimeFormat?>(Enums.DateFormat.WDDMMYYYYS, Enums.TimeFormat.HHMMD),
            new Tuple<Enums.DateFormat?, Enums.TimeFormat?>(Enums.DateFormat.DDMMYYYYD, Enums.TimeFormat.HHMMC),
            new Tuple<Enums.DateFormat?, Enums.TimeFormat?>(Enums.DateFormat.WDDMMYYYYD, Enums.TimeFormat.HHMMTTSSD),
            new Tuple<Enums.DateFormat?, Enums.TimeFormat?>(Enums.DateFormat.MMDDYYYYD, Enums.TimeFormat.HHMMTTD),
            new Tuple<Enums.DateFormat?, Enums.TimeFormat?>(Enums.DateFormat.MMDDYYYYS, Enums.TimeFormat.HHMMSSD),
            new Tuple<Enums.DateFormat?, Enums.TimeFormat?>(Enums.DateFormat.WMMDDYYYYD, Enums.TimeFormat.HHMMSSC),
            new Tuple<Enums.DateFormat?, Enums.TimeFormat?>(Enums.DateFormat.WMMDDYYYYS, Enums.TimeFormat.HHMMSSC),
        };

        var results = datetimes.SelectMany(datetime => formats.Select(format => datetime.Format(format.Item1, format.Item2)).ToArray()).ToArray();
        var expects = new[] {
            "09 Sept 2000",
            "09:09:09",
            "Sat, 09 Sept 2000, 09:09:09 am",
            "09/09/2000, 09:09:09",
            "Sat, 09/09/2000, 09.09",
            "09-09-2000, 09:09",
            "Sat, 09-09-2000, 09.09.09 am",
            "09-09-2000, 09.09 am",
            "09/09/2000, 09.09.09",
            "Sat, 09-09-2000, 09:09:09",
            "Sat, 09/09/2000, 09:09:09",
            "10 Oct 2000",
            "10:10:00",
            "Tue, 10 Oct 2000, 10:10:00 am",
            "10/10/2000, 10:10:00",
            "Tue, 10/10/2000, 10.10",
            "10-10-2000, 10:10",
            "Tue, 10-10-2000, 10.10.00 am",
            "10-10-2000, 10.10 am",
            "10/10/2000, 10.10.00",
            "Tue, 10-10-2000, 10:10:00",
            "Tue, 10/10/2000, 10:10:00",
            "11 Nov 2000",
            "00:00:00",
            "Sat, 11 Nov 2000, 12:00:00 am",
            "11/11/2000, 00:00:00",
            "Sat, 11/11/2000, 00.00",
            "11-11-2000, 00:00",
            "Sat, 11-11-2000, 12.00.00 am",
            "11-11-2000, 12.00 am",
            "11/11/2000, 00.00.00",
            "Sat, 11-11-2000, 00:00:00",
            "Sat, 11/11/2000, 00:00:00",
        };
        
        Assert.That(results, Is.EquivalentTo(expects));
    }
}
