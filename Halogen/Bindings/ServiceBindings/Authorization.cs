using HelperLibrary.Shared;

namespace Halogen.Bindings.ServiceBindings; 

public class Authorization {

    public string AccountId { get; set; } = null!;

    public Enums.Role[] Roles { get; set; } = null!;

    public string BearerToken { get; set; } = null!;

    public string AuthorizationToken { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;
    
    public long AuthorizedTimestamp { get; set; }
    
    public long ValidityDuration { get; set; }
    
    public bool? TwoFactorConfirmed { get; set; }
    
    public bool IsPreAuthorization { get; set; }
}