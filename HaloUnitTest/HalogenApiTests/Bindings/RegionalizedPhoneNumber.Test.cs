using Halogen.Bindings.ApiBindings;
using HelperLibrary.Shared;
using Newtonsoft.Json;

namespace HaloUnitTest.HalogenApiTests.Bindings;

[TestFixture]
public sealed class RegionalizedPhoneNumberTest {

    private RegionalizedPhoneNumber _phoneNumber;

    [SetUp]
    public void Setup() {
        _phoneNumber = new RegionalizedPhoneNumber {
            RegionCode = "99",
            PhoneNumber = "412357159",
        };
    }

    [Test]
    public void Test_ToString() {
        var result = _phoneNumber.ToString();
        Assert.That(result, Is.EqualTo($"{Constants.Plus}{_phoneNumber.RegionCode}{Constants.MonoSpace}{_phoneNumber.PhoneNumber}"));
    }

    [Test]
    public void Test_Simplify() {
        var result = _phoneNumber.Simplify();
        Assert.That(result, Is.EquivalentTo($"{_phoneNumber.RegionCode}{Constants.Comma}{_phoneNumber.PhoneNumber}"));
    }

    [Test]
    public void Test_ToPhoneNumber() {
        var result = _phoneNumber.ToPhoneNumber();
        Assert.That(result, Is.EquivalentTo($"{Constants.Plus}{_phoneNumber.RegionCode}{_phoneNumber.PhoneNumber}"));
    }

    [Test]
    public void Test_Convert() {
        Assert.Throws<SimplifiedRegionalPhoneNumberNoCommaException>(() => RegionalizedPhoneNumber.Convert(""));
        Assert.Throws<SimplifiedRegionalPhoneNumberNoCommaException>(() => RegionalizedPhoneNumber.Convert("+61412357468"));
        
        Assert.Throws<SimplifiedRegionalPhoneNumberTokenException>(() => RegionalizedPhoneNumber.Convert("+61,412357468,123"));
        Assert.Throws<SimplifiedRegionalPhoneNumberTokenException>(() => RegionalizedPhoneNumber.Convert(",412357468"));
        Assert.Throws<SimplifiedRegionalPhoneNumberTokenException>(() => RegionalizedPhoneNumber.Convert("61,"));
        Assert.Throws<SimplifiedRegionalPhoneNumberTokenException>(() => RegionalizedPhoneNumber.Convert(","));
        Assert.Throws<SimplifiedRegionalPhoneNumberTokenException>(() => RegionalizedPhoneNumber.Convert(" ,412468579"));
        Assert.Throws<SimplifiedRegionalPhoneNumberTokenException>(() => RegionalizedPhoneNumber.Convert("61,  "));

        var result = RegionalizedPhoneNumber.Convert("+61,412468357");
        var expect = new RegionalizedPhoneNumber {
            RegionCode = "61",
            PhoneNumber = "412468357",
        };
        Assert.That(JsonConvert.SerializeObject(result), Is.EqualTo(JsonConvert.SerializeObject(expect)));
    }

    [Test]
    public void Test_Deserialize() {
        var serialized = JsonConvert.SerializeObject(_phoneNumber);
        var result = RegionalizedPhoneNumber.Deserialize(serialized);
        Assert.That(JsonConvert.SerializeObject(result), Is.EqualTo(serialized));
    }
}
