namespace AssistantLibrary.Bindings; 

public sealed class SmsBinding {
    
    public string SmsContent { get; set; }
    
    public List<string> Receivers { get; set; }
}