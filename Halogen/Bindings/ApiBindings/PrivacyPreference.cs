using HelperLibrary.Shared;

namespace Halogen.Bindings.ApiBindings;

public sealed class ProfilePolicy {

    public bool HiddenToSearchEngines { get; set; } = true;

    public bool ViewableByStrangers { get; set; } = true;
}

public sealed class PrivacyPolicy {

    public byte DataFormat { get; set; }
    
    // If Visibility == VisibleToSomeConnections || VisibleToGroups, then PrivacyPolicy.VisibleToIds will have Ids of Accounts or Groups accordingly
    public Enums.Visibility Visibility { get; set; }
    
    public string[]? VisibleToIds { get; set; }
}
    
public sealed class SecurityPolicy {

    public bool NotifyLoginIncidentsOnUntrustedDevices { get; set; } = true;

    // if false, use Phone Number instead
    public bool PrioritizeLoginNotificationsOverEmail { get; set; }
    
    // Requires at least 1 Trusted Device to having been saved, if access lost, answer Challenge Questions to bypass (temporarily turn this off) during login
    public bool BlockLoginOnUntrustedDevices { get; set; }
}

public sealed class PrivacyPreference {

    public ProfilePolicy ProfilePreference { get; set; } = null!;

    public PrivacyPolicy NamePreference { get; set; } = null!;

    public PrivacyPolicy BirthPreference { get; set; } = null!;

    public PrivacyPolicy CareerPreference { get; set; } = null!;

    public PrivacyPolicy PhoneNumberPreference { get; set; } = null!;

    public SecurityPolicy SecurityPreference { get; set; } = null!;
}

public sealed class DataFormat {
    
    public enum DataType {
        Date,
        Time,
        Number,
        UnitSystem,
    }
    
    public DataType DtType { get; set; }
    
    public byte Format { get; set; }
}
