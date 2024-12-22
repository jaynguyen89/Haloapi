using Halogen.Bindings.ApiBindings;
using Halogen.DbModels;
using Newtonsoft.Json;

namespace Halogen.Bindings.ViewModels;

public class CredentialInformationVM {
    
    public bool? IsVerified { get; set; }
}

public sealed class EmailAddressCredentialVM: CredentialInformationVM {
    
    public string? EmailAddress { get; set; }

    public static implicit operator EmailAddressCredentialVM(Account account) => new() {
        EmailAddress = account.EmailAddress,
        IsVerified = account.EmailAddressToken is null ? null : account.EmailConfirmed,
    };
}

public sealed class PhoneNumberCredentialVM: CredentialInformationVM {
    
    public RegionalizedPhoneNumber? PhoneNumber { get; set; }

    public static implicit operator PhoneNumberCredentialVM(Profile profile) => new() {
        PhoneNumber = profile.PhoneNumber is null
            ? null
            : JsonConvert.DeserializeObject<RegionalizedPhoneNumber>(profile.PhoneNumber),
        IsVerified = profile.PhoneNumber is null || profile.PhoneNumberToken is null
            ? null
            : profile.PhoneNumberConfirmed,
    };
}
