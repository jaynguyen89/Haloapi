using Halogen.Bindings.ApiBindings;

namespace Halogen.Bindings.ViewModels;

public class CredentialInformationVM {
    
    public bool IsVerified { get; set; }
}

public sealed class EmailAddressCredentialVM: CredentialInformationVM {
    
    public string EmailAddress { get; set; }
}

public sealed class PhoneNumberCredentialVM: CredentialInformationVM {
    
    public RegionalizedPhoneNumber PhoneNumber { get; set; }
}
