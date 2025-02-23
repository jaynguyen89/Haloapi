namespace Halogen.Bindings.ApiBindings;

public class ValueData {
    
    public bool BoolValue { get; set; }
    
    public byte ByteValue { get; set; }
    
    public int? IntValue { get; set; }
    
    public string? StrValue { get; set; }
    
    public string[]? StrValues { get; set; }
    
    public Dictionary<int, string>? IntValueMaps { get; set; }
    
    public Dictionary<string, string>? StrValueMaps { get; set; }
    
    public List<KeyValuePair<int, string>>? IntValueList { get; set; }
    
    public List<KeyValuePair<string, string>>? StrValueList { get; set; }
}