using System.Text.RegularExpressions;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;

namespace Halogen.Bindings.ApiBindings;

public class LoginInformation {
    
    public string? EmailAddress { get; set; }

    public RegionalizedPhoneNumber? PhoneNumber { get; set; }

    public async Task<Dictionary<string, List<string>>> VerifyLoginInformation(RegionalizedPhoneNumberHandler phoneNumberHandler) {
        var emailAddressErrors = new List<string>();

        EmailAddress = Regex.Replace(EmailAddress?.Trim().ToLower() ?? string.Empty, Constants.MultiSpace, string.Empty);

        if (!EmailAddress.IsString() && PhoneNumber is null) emailAddressErrors.Add($"Either {nameof(EmailAddress).Lucidify()} or {nameof(PhoneNumber)} must be provided.");
        if (emailAddressErrors.Any()) return new Dictionary<string, List<string>> {{ nameof(EmailAddress), emailAddressErrors }};

        if (EmailAddress.IsString()) emailAddressErrors = emailAddressErrors.Concat(EmailAddress.VerifyEmailAddress()).ToList();
        
        var phoneNumberErrors = new List<string>();
        if (PhoneNumber is not null) phoneNumberErrors = phoneNumberErrors.Concat(await phoneNumberHandler.VerifyPhoneNumberData(PhoneNumber)).ToList();

        return ListHelpers.MergeDataValidationErrors(
            new KeyValuePair<string, List<string>>(nameof(EmailAddress), emailAddressErrors),
            new KeyValuePair<string, List<string>>(nameof(PhoneNumber), phoneNumberErrors)
        );
    }
}

public class AuthenticationData: LoginInformation {

    public string Password { get; set; } = null!;
    
    public bool IsTrusted { get; set; }
    
    public DeviceInformation? DeviceInformation { get; set; }

    public async Task<Dictionary<string, List<string>>> VerifyAuthenticationData(RegionalizedPhoneNumberHandler phoneNumberHandler) => await VerifyLoginInformation(phoneNumberHandler);
}