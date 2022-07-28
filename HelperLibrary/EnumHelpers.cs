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
    public static T? GetEnumValue<T>(this Enum any) {
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

    public static T ToEnum<T>(this string any, T defaultValue)
        where T: struct => Enum.TryParse<T>(any, true, out var result) ? result : defaultValue;
}