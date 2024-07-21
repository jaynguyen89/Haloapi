using System.Text.RegularExpressions;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;

namespace HaloUnitTest.HelperLibraryTests;

[TestFixture]
public sealed class StringHelpersTest {

    [TestCase(false)]
    [TestCase(true)]
    public void Test_NewGuid(bool longerId) {
        var guid = StringHelpers.NewGuid(longerId);

        Assert.Multiple(() => {
            Assert.That(new Regex(@"^[\dABCDEFabcdef]+$").IsMatch(guid), Is.True);
            Assert.That(guid.Length, longerId ? Is.EqualTo(64) : Is.EqualTo(32));
        });
    }

    [Test]
    public void Test_IsString() {
        var strs = new[] { null, "", " ", "   ", "+", "!@", "a", "A", "Ab" }.ToList();
        var results = strs.Select(str => str.IsString()).ToList();
        var expects = new[] { false, false, false, false, true, true, true, true, true }.ToList();
        Assert.That(results, Is.EquivalentTo(expects));
    }

    [Test]
    public void Test_RemoveAllSpaces() {
        var strs = new[] { "", "  ", "   some  thing    ", "  s0me_th1ng!    " }.ToList();
        var expects = new[] { "", "", "something", "s0me_th1ng!" }.ToList();

        var results = strs.Select(str => str.RemoveAllSpaces()).ToList();
        Assert.That(results, Is.EquivalentTo(expects));
    }

    [Test]
    public void Test_Lucidify() {
        var strs = new[] { "some", "something", "Some", "SomeThing", "Some.Thing!", "S0me.Th1ng!" };
        var expects = new[] { "some", "something", "Some", "Some Thing", "Some. Thing!", "S0me. Th1ng!" };

        var results = strs.Select(str => str.Lucidify()).ToList();
        Assert.That(results, Is.EquivalentTo(expects));
    }

    [Test]
    public void Test_CapitalizeFirstLetterOfEachWord() {
        var strs = new[] { "some", "some thing", "Some 7hing", "some Th1ng", "some thing el$e" };
        var expects = new[] { "Some", "Some Thing", "Some 7hing", "Some Th1ng", "Some Thing El$e" }.ToList();

        var results = strs.Select(str => str.CapitalizeFirstLetterOfEachWord()).ToList();
        Assert.That(results, Is.EquivalentTo(expects));
    }

    [Test]
    public void Test_LowerCaseFirstChar() {
        var strs = new[] { "some", "Some", "$ome", "8ome", "Some th1ng", "Some 7h!ng", "Some Thing" };
        var expects = new[] { "some", "some", "$ome", "8ome", "some th1ng", "some 7h!ng", "some Thing" }.ToList();

        var results = strs.Select(str => str.LowerCaseFirstChar()).ToList();
        Assert.That(results, Is.EquivalentTo(expects));
    }

    [Test]
    public void Test_UpperCaseFirstChar() {
        var strs = new[] { "some", "Some", "$ome", "8ome", "some th1ng", "some 7h!ng", "some Thing" };
        var expects = new[] { "Some", "Some", "$ome", "8ome", "Some th1ng", "Some 7h!ng", "Some Thing" };

        var results = strs.Select(str => str.UpperCaseFirstChar()).ToList();
        Assert.That(results, Is.EquivalentTo(expects));
    }

    [Test]
    public void Test_RemoveAllSpecialChars() {
        var strs = new[] { "N0 sp3C1al Ch5rs", ">W!th ~(all*)+% `$Speci@l #{chars} n& nu^mber\\s => [t0-be_re|moved]: ;\"'<./?" };
        var expects = new[] { "N0 sp3C1al Ch5rs", "Wth all Specil chars n numbers  t0beremoved " }.ToList();

        var results = strs.Select(str => str.RemoveAllSpecialChars()).ToList();
        Assert.That(results, Is.EquivalentTo(expects));
    }

    [TestCase(false, true)]
    [TestCase(false, false)]
    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true, 32)]
    [TestCase(false, false, 48)]
    [TestCase(true, true, 24)]
    [TestCase(true, false, 56)]
    public void Test_GenerateRandomString(bool specialChar, bool caseSensitive, int len = Constants.RandomStringDefaultLength) {
        var result = StringHelpers.GenerateRandomString(len, specialChar, caseSensitive);
        
        Assert.That(result, Has.Length.EqualTo(len));
        Assert.That(result, !specialChar ? Is.EqualTo(result.RemoveAllSpecialChars()) : Is.Not.EqualTo(result.RemoveAllSpecialChars()));
        Assert.That(result, !caseSensitive ? Is.EqualTo(result.ToUpper()) : Is.Not.EqualTo(result.ToUpper()));
    }

    [TestCase(-1)]
    [TestCase(0, Constants.MonoSpace)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3, Constants.Underscore)]
    [TestCase(5, Constants.Comma)]
    [TestCase(6)]
    public void Test_SplitToGroups(int groups, string separator = Constants.Hyphen) {
        const string str = "1a2b3c4d5e6f7g8h9i0j";
        var result = str.SplitToGroups(groups, separator);
        
        switch (groups) {
            case < 2:
                Assert.That(result, Is.EqualTo(str));
                break;
            case 2:
                Assert.That(result, Is.EqualTo($"1a2b3c4d5e{separator}6f7g8h9i0j"));
                break;
            case 3:
                Assert.That(result, Is.EqualTo($"1a2b3c{separator}4d5e6f{separator}7g8h9i0j"));
                break;
            case 5:
                Assert.That(result, Is.EqualTo($"1a2b{separator}3c4d{separator}5e6f{separator}7g8h{separator}9i0j"));
                break;
            case 6:
                Assert.That(result, Is.EqualTo($"1a2{separator}b3c{separator}4d5{separator}e6f{separator}7g8{separator}h9i0j"));
                break;
        }
    }

    [Test]
    public void Test_ShortOrLong() {
        var strs = new[] { "Test", "Testing Something", "Testing Something Else" };
        var results = strs.Select(str => str.ShortOrLong(10, 20)).ToList();

        var expects = new[] { "short", "", "long" }.ToList();
        Assert.That(results, Is.EquivalentTo(expects));
    }

    [Test]
    public void Test_IsValidUrl() {
        var urls = new[] {
            "http://som3.com.ex?so=so%20%me&me=[1,2]&el=some.png&yes=true",
            "https://www.so-m3.com/ex/s0-me?r=123&p=path-to-somewhere",
            "ftp://192.163.10.29/go/to/somewhere",
            "ftps://do-ma1n.com/path/to/dir?param=1234&some=false",
            "sftp://11.22.33.44?where=s0%20%m3",
            "www.s0m3-where.com.ex/dir/sub?so=me&wh=ere&p=[0,1,2]&r=pic.jpg",
            "not-an-url",
            "D:/path/to/folder",
        };

        var results = urls.Select(url => url.IsValidUrl()).ToList();
        var expects = new[] { true, true, true, true, true, true, false, false }.ToList();
        
        Assert.That(results, Is.EquivalentTo(expects));
    }

    [Test]
    public void Test_SetDefaultEmailBodyValues() {
        const string body = "This is an email body." +
                            "Logo: <a>CLIENT_LOGO_URL</a>" +
                            "Base URI: <a>CLIENT_BASE_URI</a>" +
                            "App Name: \"CLIENT_APPLICATION_NAME\"" +
                            "Time: 12/12/CURRENT_YEAR 12:12 PM";

        string logo = "https://host.com/img.svg", uri = "https://localhost:3000", name = "Test App";
        var result = body.SetDefaultEmailBodyValues(new Tuple<string, string, string>(logo, uri, name));
        
        var expect = "This is an email body." +
                     $"Logo: <a>{logo}</a>" +
                     $"Base URI: <a>{uri}</a>" +
                     $"App Name: \"{name}\"" +
                     $"Time: 12/12/{DateTime.UtcNow.Year} 12:12 PM";
        
        Assert.That(result, Is.EqualTo(expect));
    }
}
