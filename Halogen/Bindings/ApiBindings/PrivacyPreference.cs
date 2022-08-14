using HelperLibrary.Shared;

namespace Halogen.Bindings.ApiBindings; 

public class VisibilityPolicy {
        
    public Enums.Visibility Visibility { get; set; }
}

public interface IPolicyWithDataFormat {
        
    byte DataFormat { get; set; }
}

public interface IPolicyWithSingleTarget {
    
    string? VisibleToTargetId { get; set; }
        
    string? TargetTypeName { get; set; }
}

public interface IPolicyWithMultipleTargets {
    
    string[] VisibleToTargetIds { get; set; }
        
    string TargetTypeName { get; set; }
}

public interface IPolicyWithMultipleTypedTargets {
    
    TypedTarget[] TypedTargets { get; set; }
    
    public sealed class TypedTarget {

        public string[] VisibleToTargetIds { get; set; } = null!;

        public string TargetTypeName { get; set; } = null!;
    }
}

public sealed class PrivacyPreference {

    public ProfilePolicy ProfilePreference { get; set; } = null!;

    public PrivacyPolicy NamePreference { get; set; } = null!;

    public PrivacyPolicy BirthPreference { get; set; } = null!;

    public PrivacyPolicy CareerPreference { get; set; } = null!;

    public Enums.Visibility PhoneNumberVisibility { get; set; }

    public SecurityPolicy SecurityPreference { get; set; } = null!;
}
    
public sealed class PrivacyPolicy: VisibilityPolicy, IPolicyWithDataFormat, IPolicyWithSingleTarget {

    public byte DataFormat { get; set; }

    public string? VisibleToTargetId { get; set; }

    public string? TargetTypeName { get; set; }
}
    
public sealed class ProfilePolicy {
    
    public bool HiddenToSearchEngines { get; set; }
    
    public bool HiddenToStrangers { get; set; }
    
    public string? ReachableByTypeName { get; set; }
}
    
public sealed class SecurityPolicy {

    public bool NotifyLoginIncidentsOnUntrustedDevices { get; set; } = true;

    public bool PrioritizeLoginNotificationsOverEmail { get; set; } = true; // if false, use Phone Number instead
    
    public bool BlockLoginOnUntrustedDevices { get; set; } // Requires at least 1 Trusted Device to having been saved, if access lost, answer challenge questions to bypass (temporarily turn this off) during login
}