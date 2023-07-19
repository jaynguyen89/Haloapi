namespace Halogen.Bindings.ApiBindings; 

public sealed class TwoFactorKeys {
    
    public string SecretKey { get; set; } = null!;

    public string ManualEntryKey { get; set; } = null!;
}