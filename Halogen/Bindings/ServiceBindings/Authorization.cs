namespace Halogen.Bindings.ServiceBindings; 

public sealed class Authorization {

    public string AccountId { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;

    public string BearerToken { get; set; } = null!;

    public string AuthorizationToken { get; set; } = null!;
    
    public long AuthorizedTimestamp { get; set; }
    
    public long ValidityDuration { get; set; }
}