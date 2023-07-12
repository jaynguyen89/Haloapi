using System.Reflection;

namespace HelperLibrary;

[AttributeUsage(AttributeTargets.Field)]
public class ValueAttribute : Attribute {
    public string StringValue { get; set; }

    public ValueAttribute(string stringValue) {
        StringValue = stringValue;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class CompositeValueAttribute: ValueAttribute {
    public byte ByteValue { get; set; }

    public CompositeValueAttribute(string stringValue, byte byteValue): base(stringValue) {
        ByteValue = byteValue;
    }
}

public static class EnumHelpers {
    
    /// <summary>
    /// To get StringValue and ByteValue of enums having CompositeValue attribute.
    /// Or to get StringValue of enums having Value attribute.
    /// </summary>
    public static T? GetCompositeValue<T>(this Enum any) {
        var enumType = any.GetType();
        var enumFields = enumType.GetField(any.ToString());

        if (enumFields is null) return default;

        if (typeof(T).Name.ToLower().Equals(nameof(String).ToLower())) {
            var stringValue = enumFields.GetCustomAttributes(typeof(CompositeValueAttribute), false)
                is CompositeValueAttribute[] { Length: > 0 } attributesForStringValue
                ? attributesForStringValue[0].StringValue
                : string.Empty;
                
            return string.IsNullOrEmpty(stringValue)
                ? default
                : (T) Convert.ChangeType(stringValue, TypeCode.String);
        }

        var byteValue = enumFields.GetCustomAttributes(typeof(CompositeValueAttribute), false)
            is CompositeValueAttribute[] { Length: > 0 } attributesForByteValue
            ? attributesForByteValue[0].ByteValue
            : byte.MinValue;
            
        return (T) Convert.ChangeType(byteValue, TypeCode.Byte);
    }

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
    /// <param name="defaultValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T ToEnum<T>(this string any, T defaultValue)
        where T: struct => Enum.TryParse<T>(any, true, out var result) ? result : defaultValue;
    
    public static T? GetEnumByValueAttribute<T>(this string value) {
        var enumType = typeof(T);
        try {
            foreach (T? val in Enum.GetValues(enumType)) {
                var fieldInfo = enumType.GetField(val.ToString()!);
                var attributes = (ValueAttribute[])fieldInfo!.GetCustomAttributes(typeof(ValueAttribute), false);

                var attr = attributes[0];
                if (attr.StringValue == value) return val;
            }

            return default;
        }
        catch (ArgumentException) {
            return default;
        }
    }

    public static string[] GetAllEnumProperties<T>() => (string[])typeof(T)
        .GetFields()
        .Select(x => new { 
            att = x.GetCustomAttributes(false)
                .OfType<ValueAttribute>()
                .FirstOrDefault(),
            x,
        })
        .Where(x => x.att != null)
        .Select(x => x.x.GetValue(null))
        .ToArray();
}