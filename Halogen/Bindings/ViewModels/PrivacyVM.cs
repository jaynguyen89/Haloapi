using Halogen.Bindings.ApiBindings;
using HelperLibrary.Shared;

namespace Halogen.Bindings.ViewModels;

public sealed class PrivacyPolicyVM {
    
    public byte DataFormat { get; set; }
    
    public Enums.Visibility Visibility { get; set; }
    
    public VisibleToVM[]? VisibleTos { get; set; }

    public sealed class VisibleToVM {

        public string Id { get; set; } = null!;
        
        public string Username { get; set; } = null!;
        
        public string? Name { get; set; }
    }
}

public sealed class SecurityPolicyVM {
    
    public bool NotifyLoginIncidentsOnUntrustedDevices { get; set; }

    public bool NotifyLoginIncidentsOverEmail { get; set; }
    
    public bool CanChangeNotifyLoginIncidentsOverEmail { get; set; }
    
    public bool BlockLoginOnUntrustedDevices { get; set; }
    
    public bool CanChangeBlockLoginOnUntrustedDevices { get; set; }
}

public sealed class PrivacyVM {
    
    public ProfilePolicy ProfilePreference { get; set; } = null!;

    public PrivacyPolicyVM NamePreference { get; set; } = null!;

    public PrivacyPolicyVM BirthPreference { get; set; } = null!;

    public PrivacyPolicyVM CareerPreference { get; set; } = null!;

    public PrivacyPolicyVM PhoneNumberPreference { get; set; } = null!;

    public SecurityPolicyVM SecurityPreference { get; set; } = null!;
}