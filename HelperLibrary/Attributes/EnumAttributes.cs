namespace HelperLibrary.Attributes;

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
