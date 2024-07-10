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
        Assert.That(result, Is.EqualTo(expect));
    }

    [Test]
    public void Test_ToTimestamp() {
        
    }

    [Test]
    public void Test_ToMilliseconds() {
        
    }

    [Test]
    public void Test_Format() {
        
    }
}