namespace HelperLibrary.Shared; 

public sealed class EnumValueAttribute: Attribute {
    
    public string StringValue { get; set; }
        
    public byte ByteValue { get; set; }

    public EnumValueAttribute(string stringValue, byte byteValue) {
        StringValue = stringValue;
        ByteValue = byteValue;
    }
}