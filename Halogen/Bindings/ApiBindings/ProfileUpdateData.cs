namespace Halogen.Bindings.ApiBindings;

public sealed class ProfileUpdateData {
    
    public string FieldName { get; set; }
    
    public string? Value { get; set; }
    
    public string[]? Values { get; set; }

    public List<string> VerifyProfileUpdateData() {
        
    }
}