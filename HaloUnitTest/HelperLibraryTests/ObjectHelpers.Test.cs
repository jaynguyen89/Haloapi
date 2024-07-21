using HelperLibrary.Shared.Helpers;

namespace HaloUnitTest.HelperLibraryTests;

public sealed class ObjectHelpersTest {

    [Test]
    public void Test_EncodeDataAscii() {
        var num = 12345;
        var numCode = num.EncodeDataAscii();
        
        Assert.That(numCode, Is.EquivalentTo(new byte[] {49, 50, 51, 52, 53}));
        
        var str = "S0meth!ng";
        var strCode = str.EncodeDataAscii();
        
        Assert.That(strCode, Is.EquivalentTo(new byte[] {34, 83, 48, 109, 101, 116, 104, 33, 110, 103, 34}));
        
        var data = new { Key1 = "Value", Key2 = new { K1 = "V", K2 = new[] { "some" } }, Key3 = 123 };
        var result = data.EncodeDataAscii();

        var expect = new byte[] {123, 34, 75, 101, 121, 49, 34, 58, 34, 86, 97, 108, 117, 101, 34, 44, 34, 75, 101, 121, 50, 34, 58, 123, 34, 75, 49, 34, 58, 34, 86, 34, 44, 34, 75, 50, 34, 58, 91, 34, 115, 111, 109, 101, 34, 93, 125, 44, 34, 75, 101, 121, 51, 34, 58, 49, 50, 51, 125};
        Assert.That(result, Is.EquivalentTo(expect));
    }
    
    [Test]
    public void Test_EncodeDataUtf8() {
        var num = 12345;
        var numCode = num.EncodeDataUtf8();
        
        Assert.That(numCode, Is.EquivalentTo(new byte[] {49, 50, 51, 52, 53}));
        
        var str = "S0meth!ng";
        var strCode = str.EncodeDataUtf8();
        
        Assert.That(strCode, Is.EquivalentTo(new byte[] {34, 83, 48, 109, 101, 116, 104, 33, 110, 103, 34}));
        
        var data = new { Key1 = "Value", Key2 = new { K1 = "V", K2 = new[] { "some" } }, Key3 = 123 };
        var result = data.EncodeDataUtf8();

        var expect = new byte[] {123, 34, 75, 101, 121, 49, 34, 58, 34, 86, 97, 108, 117, 101, 34, 44, 34, 75, 101, 121, 50, 34, 58, 123, 34, 75, 49, 34, 58, 34, 86, 34, 44, 34, 75, 50, 34, 58, 91, 34, 115, 111, 109, 101, 34, 93, 125, 44, 34, 75, 101, 121, 51, 34, 58, 49, 50, 51, 125};
        Assert.That(result, Is.EquivalentTo(expect));
    }
    
    [Test]
    public void Test_DecodeUtf8() {
        var numCode = new byte[] {49, 50, 51, 52, 53};
        var num = numCode.DecodeUtf8<int>();
        
        Assert.That(num, Is.EqualTo(12345));
        
        var strCode = new byte[] {34, 83, 48, 109, 101, 116, 104, 33, 110, 103, 34};
        var str = strCode.DecodeUtf8<string>();
        
        Assert.That(str, Is.EqualTo("S0meth!ng"));
        
        // var bytes = new byte[] {123, 34, 75, 101, 121, 49, 34, 58, 34, 86, 97, 108, 117, 101, 34, 44, 34, 75, 101, 121, 50, 34, 58, 123, 34, 75, 49, 34, 58, 34, 86, 34, 44, 34, 75, 50, 34, 58, 91, 34, 115, 111, 109, 101, 34, 93, 125, 44, 34, 75, 101, 121, 51, 34, 58, 49, 50, 51, 125};
        // var result = bytes.DecodeUtf8<object>();
        //
        // var expect = new { Key1 = "Value", Key2 = new { K1 = "V", K2 = new[] { "some" } }, Key3 = 123 };
        // Assert.That(result, Is.SameAs(expect));
    }
}