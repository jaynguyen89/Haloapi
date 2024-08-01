using HelperLibrary.Attributes;
using HelperLibrary.Shared.Helpers;

namespace HaloUnitTest.HelperLibraryTests;

[TestFixture]
public sealed class EnumHelpersTest {
    
    private enum SimpleEnum {
        Key1,
        Key2,
    }
    
    private enum RegularEnum {
        Key1 = 1,
        Key2 = 3,
    }
    
    private enum ValueEnum {
        [Value("Value 1")]
        Key1,
        [Value("Value 2")]
        Key2,
    }

    private enum CompositeEnum {
        [CompositeValue("Value 1", "Code 1")]
        Key1,
        [CompositeValue("Value 2", "Code 2")]
        Key2,
    }

    [Test]
    public void Test_Length() {
        var length = EnumHelpers.Length<SimpleEnum>();
        Assert.That(length, Is.EqualTo(2));

        length = EnumHelpers.Length<RegularEnum>();
        Assert.That(length, Is.EqualTo(2));
        
        length = EnumHelpers.Length<ValueEnum>();
        Assert.That(length, Is.EqualTo(2));

        length = EnumHelpers.Length<CompositeEnum>();
        Assert.That(length, Is.EqualTo(2));
    }

    [Test]
    public void Test_GetCompositeValue() {
        var value = CompositeEnum.Key1.GetCompositeValue();
        Assert.That(value, Is.EqualTo("Value 1"));

        value = CompositeEnum.Key2.GetCompositeValue();
        Assert.That(value, Is.EqualTo("Value 2"));

        var code = CompositeEnum.Key1.GetCompositeValue(false);
        Assert.That(code, Is.EqualTo("Code 1"));

        code = CompositeEnum.Key2.GetCompositeValue(false);
        Assert.That(code, Is.EqualTo("Code 2"));
    }

    [Test]
    public void Test_GetValue() {
        var value = ValueEnum.Key1.GetValue();
        Assert.That(value, Is.EqualTo("Value 1"));

        value = ValueEnum.Key2.GetValue();
        Assert.That(value, Is.EqualTo("Value 2"));
    }

    [Test]
    public void Test_ToEnum() {
        var simpleKey = "Key0".ToEnum<SimpleEnum>();
        Assert.That(simpleKey, Is.Null);

        simpleKey = "Key1".ToEnum<SimpleEnum>();
        Assert.That(simpleKey, Is.EqualTo(SimpleEnum.Key1));

        var regularKey = "Key0".ToEnum<RegularEnum>();
        Assert.That(regularKey, Is.Null);

        regularKey = "Key1".ToEnum<RegularEnum>();
        Assert.That(regularKey, Is.EqualTo(RegularEnum.Key1));

        var valueKey = "Key0".ToEnum<ValueEnum>();
        Assert.That(valueKey, Is.Null);

        valueKey = "Key1".ToEnum<ValueEnum>();
        Assert.That(valueKey, Is.EqualTo(ValueEnum.Key1));

        var compositeKey = "Key0".ToEnum<CompositeEnum>();
        Assert.That(compositeKey, Is.Null);

        compositeKey = "Key1".ToEnum<CompositeEnum>();
        Assert.That(compositeKey, Is.EqualTo(CompositeEnum.Key1));
    }

    [Test]
    public void Test_GetEnumByValueAttribute() {
        var valueKey = "Value 0".GetEnumByValueAttribute<ValueEnum>();
        Assert.That(valueKey, Is.Null);

        valueKey = "Value 2".GetEnumByValueAttribute<ValueEnum>();
        Assert.That(valueKey, Is.EqualTo(ValueEnum.Key2));

        var compositeKey = "Value 0".GetEnumByValueAttribute<CompositeEnum>();
        Assert.That(compositeKey, Is.Null);

        compositeKey = "Value 2".GetEnumByValueAttribute<CompositeEnum>();
        Assert.That(compositeKey, Is.EqualTo(CompositeEnum.Key2));
    }

    [Test]
    public void Test_GetAllPropertiesForSimpleEnums() {
        var expect = new[] { "Key1", "Key2" };
        
        var simpleProps = EnumHelpers.GetAllPropertiesForSimpleEnums<SimpleEnum>();
        Assert.That(simpleProps, Is.EquivalentTo(expect));
        
        var regularProps = EnumHelpers.GetAllPropertiesForSimpleEnums<RegularEnum>();
        Assert.That(regularProps, Is.EquivalentTo(expect));
    }

    [Test]
    public void Test_GetAllPropertiesForAttributedEnums() {
        var expect = new[] { "Key1", "Key2" };

        var valueProps = EnumHelpers.GetAllPropertiesForAttributedEnums<ValueEnum>();
        Assert.That(valueProps, Is.EquivalentTo(expect));

        var compositeProps = EnumHelpers.GetAllPropertiesForAttributedEnums<CompositeEnum>();
        Assert.That(compositeProps, Is.EquivalentTo(expect));
    }

    [Test]
    public void Test_ToDictionaryWithValueAttribute() {
        var expect = new[] {
            new EnumProp {
                Index = 0,
                Display = "Value 1",
            },
            new EnumProp {
                Index = 1,
                Display = "Value 2",
            },
        }.ToList();
        
        var values = EnumHelpers.ToArrayWithValueAttribute<ValueEnum>().ToList();
        
        var i = 0;
        expect.ForEach(each => {
            var j = i;
            Assert.Multiple(() => {
                Assert.That(values.ElementAt(j).Display, Is.EqualTo(each.Display));
                Assert.That(values.ElementAt(j).Index, Is.EqualTo(each.Index));
                Assert.That(values.ElementAt(j).Code, Is.Null);
            });
            i++;
        });
    }

    [Test]
    public void Test_ToArrayWithCompositeAttribute() {
        var expect = new[] {
            new EnumProp {
                Code = "Code 1",
                Display = "Value 1",
            },
            new EnumProp {
                Code = "Code 2",
                Display = "Value 2",
            },
        }.ToList();

        var composites = EnumHelpers.ToArrayWithCompositeAttribute<CompositeEnum>();

        var i = 0;
        expect.ForEach(each => {
            var j = i;
            Assert.Multiple(() => {
                Assert.That(composites.ElementAt(j).Display, Is.EqualTo(each.Display));
                Assert.That(composites.ElementAt(j).Index, Is.Null);
                Assert.That(composites.ElementAt(j).Code, Is.EqualTo(each.Code));
            });
            i++;
        });
    }
}
