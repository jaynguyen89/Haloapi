using Halogen.Bindings.ApiBindings;
using HelperLibrary.Shared;

namespace Halogen.Bindings.ViewModels;

public sealed class AuthenticatedUser {

    public string AccountId { get; set; } = null!;

    public string ProfileId { get; set; } = null!;

    public string Username { get; set; } = null!;
    
    public Enums.Role Role { get; set; }
    
    public string? FullName { get; set; }
    
    public string? EmailAddress { get; set; }
    
    public RegionalizedPhoneNumber? PhoneNumber { get; set; }
}
