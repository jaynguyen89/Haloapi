using System.Reflection;

namespace HelperLibrary.Shared.Helpers;

[AttributeUsage(AttributeTargets.Field)]
public class ValueAttribute : Attribute {
    public string StringValue { get; set; }

    public ValueAttribute(string stringValue) {
        StringValue = stringValue;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class CompositeValueAttribute: ValueAttribute {
    public string CodeValue { get; set; }

    public CompositeValueAttribute(string stringValue, string codeValue): base(stringValue) {
        CodeValue = codeValue;
    }
}

/// <summary>
/// The 2 classes below are used to convert the Enums into object with EnumProp schema
/// </summary>
public sealed class EnumProp {
        
    public int? Index { get; set; }
    
    public string? Code { get; set; }

    public string Display { get; set; } = null!;
}

public static class EnumHelpers {

    public static byte Length<T>() where T: Enum => (byte)Convert.ChangeType(Enum.GetNames(typeof(T)).Length, TypeCode.Byte);
    
    /// <summary>
    /// To get StringValue and ByteValue of enums having CompositeValue attribute.
    /// </summary>
    public static string? GetCompositeValue(this Enum any, bool getStringValue = true) {
        var enumType = any.GetType();
        var enumFields = enumType.GetField(any.ToString());

        if (enumFields is null) return default;
        
        var stringValue = enumFields.GetCustomAttributes(typeof(CompositeValueAttribute), false)
            is CompositeValueAttribute[] { Length: > 0 } attributesForStringValue
            ? getStringValue ? attributesForStringValue[0].StringValue : attributesForStringValue[0].CodeValue
            : string.Empty;
            
        return string.IsNullOrEmpty(stringValue)
            ? default
            : (string) Convert.ChangeType(stringValue, TypeCode.String);
    }

    /// <summary>
    /// To get StringValue of enums having Value attribute.
    /// </summary>
    public static string? GetValue(this Enum any) {
        var enumType = any.GetType();
        var enumFields = enumType.GetField(any.ToString());
        
        if (enumFields is null) return default;
        
        return enumFields.GetCustomAttributes(typeof(ValueAttribute), false)
            is ValueAttribute[] { Length: > 0 } attributesForStringValue
            ? attributesForStringValue[0].StringValue
            : string.Empty;
    }

    /// <summary>
    /// Convert a string to an enum property provided the string matches 1 property of the enum
    /// </summary>
    /// <param name="any"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? ToEnum<T>(this string any)
        where T: struct => Enum.TryParse<T>(any, true, out var result) ? result : null;
    
    public static T? GetEnumByValueAttribute<T>(this string value) where T: struct {
        var enumType = typeof(T);
        try {
            foreach (T? val in Enum.GetValues(enumType)) {
                var fieldInfo = enumType.GetField(val.ToString()!);
                var attributes = (ValueAttribute[])fieldInfo!.GetCustomAttributes(typeof(ValueAttribute), false);

                if (attributes[0].StringValue == value) return val;
            }

            return null;
        }
        catch (ArgumentException) {
            return null;
        }
    }

    public static string[] GetAllPropertiesForSimpleEnums<T>() where T: Enum =>
        typeof(T).GetFields().Select(field => field.Name).Skip(1).ToArray();

    public static string[] GetAllPropertiesForAttributedEnums<T>() where T: Enum => typeof(T)
        .GetFields()
        .Select(fieldInfo => new { 
            ValueAttribute = fieldInfo.GetCustomAttributes(false)
                .OfType<ValueAttribute>()
                .FirstOrDefault(),
            FieldInfo = fieldInfo,
        })
        .Where(attribute => attribute.ValueAttribute is not null)
        .Select(attribute => attribute.FieldInfo.Name)
        .ToArray();

    public static EnumProp[] ToArrayWithValueAttribute<T>() where T: Enum => (
        from T val in Enum.GetValues(typeof(T))
        let fieldInfo = typeof(T).GetField(val.ToString()!)
        let attributes = (ValueAttribute[])fieldInfo!.GetCustomAttributes(typeof(ValueAttribute), false)
        select new EnumProp {
            Index = (int)(object)val,
            Display = attributes[0].StringValue,
        }).ToArray();
    
    public static EnumProp[] ToArrayWithCompositeAttribute<T>() where T: Enum => (
        from T val in Enum.GetValues(typeof(T))
        let fieldInfo = typeof(T).GetField(val.ToString()!)
        let attributes = (CompositeValueAttribute[])fieldInfo!.GetCustomAttributes(typeof(CompositeValueAttribute), false)
        select new EnumProp {
            Code = attributes[0].CodeValue,
            Display = attributes[0].StringValue,
        }).ToArray();
}