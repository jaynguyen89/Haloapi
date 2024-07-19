using HelperLibrary.Shared.Helpers;
using Newtonsoft.Json;

namespace HaloUnitTest.HelperLibraryTests;

[TestFixture]
public sealed class ListHelpersTest {

    private readonly Dictionary<int, string?> strDict = new() {
        {1, "One"}, {3, null}, {5, "Five"}
    };

    private readonly Dictionary<string, object?> objDict = new() {
        {"one", new { k1 = 1, k2 = "k2" }}, {"two", new { t1 = "t1", t2 = 2 }}, {"three", null}
    };

    private readonly Dictionary<int, List<object>> listDict = new() {
        {1, [new { k1 = "k1" }, new { k2 = "k2" }] }, {2, [new { t1 = "t1", t2 = 1 }, new { m1 = 1, m2 = "m2" }]}
    };

    [Test]
    public void Test_GetDictionaryValue() {
        Assert.Throws<KeyNotFoundException>(() => strDict.GetDictionaryValue(0));
        Assert.Throws<KeyNotFoundException>(() => objDict.GetDictionaryValue("none"));
        Assert.Throws<KeyNotFoundException>(() => listDict!.GetDictionaryValue(0));

        var strVal = strDict.GetDictionaryValue(1);
        Assert.That(strVal, Is.EqualTo("One"));

        strVal = strDict.GetDictionaryValue(3);
        Assert.That(strVal, Is.Null);

        var objVal = objDict.GetDictionaryValue("two");
        Assert.That(JsonConvert.SerializeObject(objVal), Is.EqualTo(JsonConvert.SerializeObject(new { t1 = "t1", t2 = 2 })));

        objVal = objDict.GetDictionaryValue("three");
        Assert.That(objVal, Is.Null);

        var listVal = listDict!.GetDictionaryValue(2);
        Assert.That(listVal, Is.EquivalentTo(new List<object> {new { t1 = "t1", t2 = 1 }, new { m1 = 1, m2 = "m2" }}));
    }

    [Test]
    public void Test_MergeDataValidationErrors() {
        var result = ListHelpers.MergeDataValidationErrors(
            new KeyValuePair<string, List<string>>("Field 1", ["Error 1"]),
            new KeyValuePair<string, List<string>>("Field 2", ["Problem 1", "Problem 2"]),
            new KeyValuePair<string, List<string>>("Field 3", ["Issue 1", "Issue 2", "Issue 3"])
        );

        var expect = new Dictionary<string, List<string>> {
            {"Field 1", ["Error 1"]},
            {"Field 2", ["Problem 1", "Problem 2"]},
            {"Field 3", ["Issue 1", "Issue 2", "Issue 3"]},
        };
        
        Assert.That(result, Is.EquivalentTo(expect));
    }

    [Test]
    public void Test_Overloaded_MergeDataValidationErrors() {
        var errors = new Dictionary<string, List<string>> {
            {"Field", ["Validation"]}
        };
        
        errors.MergeDataValidationErrors(
            new KeyValuePair<string, List<string>>("Field 1", ["Error 1"]),
            new KeyValuePair<string, List<string>>("Field 2", ["Problem 1", "Problem 2"]),
            new KeyValuePair<string, List<string>>("Field 3", ["Issue 1", "Issue 2", "Issue 3"])
        );
        
        var expect = new Dictionary<string, List<string>> {
            {"Field", ["Validation"]},
            {"Field 1", ["Error 1"]},
            {"Field 2", ["Problem 1", "Problem 2"]},
            {"Field 3", ["Issue 1", "Issue 2", "Issue 3"]},
        };
        
        Assert.That(errors, Is.EquivalentTo(expect));
    }
}
