namespace Halogen.Bindings.ApiBindings; 

public sealed class DeviceInformation {
    
    public string? Name { get; set; }
    
    public string? DeviceType { get; set; } // PC, Mac, iPhone, iPad, Android tablet
    
    public string? UniqueIdentifier { get; set; } // May be any unique device ID (ie. Serial No., Product Code, PIN...)
    
    public string? UniqueIdentifierType { get; set; }
    
    public string? Location { get; set; }
    
    public string? IpAddress { get; set; }
    
    public string? OperatingSystem { get; set; }
    
    public string? BrowserType { get; set; }
}