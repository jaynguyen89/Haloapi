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
        };

        var results = durations.Select(duration => duration.Value.ToMilliseconds(duration.Key)).ToList();
        var expects = new[] {
            1234567890,
            13257000,
            534060000,
            8636400000,
            4752000000,
            6652800000,
        };
        
        results.ForEach(result => Assert.That(result, Is.EqualTo(expects[results.IndexOf(result)])));
    }

    [Test]
    public void Test_Compute() {
        
    }

    [Test]
    public void Test_Format() {
        
    }
}