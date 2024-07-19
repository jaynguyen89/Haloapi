using HelperLibrary.Shared.Helpers;

namespace HaloUnitTest.HelperLibraryTests;

[TestFixture]
public sealed class NumberHelpersTest {

    [Test]
    public void Test_GetRandomNumberInRangeInclusive() {
        Assert.Throws<ArgumentException>(() => NumberHelpers.GetRandomNumberInRangeInclusive(1, 1));
        Assert.Throws<ArgumentException>(() => NumberHelpers.GetRandomNumberInRangeInclusive(1, 0));
        
        var ranges = new List<KeyValuePair<int, int>> {
            new(-10, 0),
            new(0, 1),
            new(1, 10),
        };

        var results = ranges.Select(range => NumberHelpers.GetRandomNumberInRangeInclusive(range.Key, range.Value)).ToList();
        results.ForEach(result => Assert.That(result, Is.InRange(ranges.ElementAt(results.IndexOf(result)).Key, ranges.ElementAt(results.IndexOf(result)).Value)));
    }

    [TestCase("")]
    [TestCase("x")]
    [TestCase(" ")]
    [TestCase("-")]
    public void Test_IsNumberFalse(string input) {
        var isNumber = input.IsNumber();
        Assert.That(isNumber, Is.False);
    }
    
    [TestCase("1")]
    [TestCase("0")]
    [TestCase("0.0")]
    [TestCase("1.1")]
    [TestCase("-0")]
    [TestCase("-1")]
    [TestCase("-0.0")]
    [TestCase("-0.1")]
    [TestCase("0.1")]
    [TestCase("+0.1")]
    [TestCase("+0")]
    [TestCase("+1")]
    [TestCase("+1.0")]
    [TestCase("+0.0")]
    [TestCase("-01")]
    [TestCase("+01")]
    [TestCase("01")]
    public void Test_IsNumberTrue(string input) {
        var isNumber = input.IsNumber();
        Assert.That(isNumber, Is.True);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("x")]
    [TestCase(" ")]
    [TestCase("-")]
    public void Test_ToIntNull(string? input) {
        var num = input.ToInt();
        Assert.That(num, Is.Null);
    }

    [Test]
    public void Test_ToInt() {
        var inputs = new[] { "1", "0", "-0", "-1", "+0", "+1", "-01", "+01", "01" }.ToList();
        var expects = new[] { 1, 0, 0, -1, 0, 1, -1, 1, 1 }.ToList();

        var results = inputs.Select(input => input.ToInt()).ToList();
        Assert.That(results, Is.EquivalentTo(expects));
    }
    
    [TestCase(null)]
    [TestCase("")]
    [TestCase("x")]
    [TestCase(" ")]
    [TestCase("-")]
    public void Test_ToDoubleNull(string? input) {
        var num = input.ToDouble();
        Assert.That(num, Is.Null);
    }
    
    [Test]
    public void Test_ToDouble() {
        var inputs = new[] { "0.0", "1.1", "-0.0", "-0.1", "0.1", "+0.1", "+1.0", "+0.0", "0", "1" }.ToList();
        var expects = new[] { 0.0, 1.1, 0.0, -0.1, 0.1, 0.1, 1.0, 0.0, 0.0, 1.0 }.ToList();

        var results = inputs.Select(input => input.ToDouble()).ToList();
        Assert.That(results, Is.EquivalentTo(expects));
    }
}
