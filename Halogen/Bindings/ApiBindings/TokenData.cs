using HelperLibrary.Shared;

namespace Halogen.Bindings.ApiBindings;

public sealed class TokenData {
    
    public Enums.TokenDestination Destination { get; set; } // forward-token, renew-token
    
    public bool IsOtp { get; set; } // forward-token
    
    public string? SecretCode { get; set; } // forward-token, renew-token, activate-account
    
    public string? CurrentToken { get; set; } // renew-token, activate-account
}