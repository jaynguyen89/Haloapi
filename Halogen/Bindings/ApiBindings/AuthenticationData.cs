using System.Text.RegularExpressions;
using HelperLibrary;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;

namespace Halogen.Bindings.ApiBindings;

public class LoginInformation {
    
    public string? EmailAddress { get; set; }

    public RegionalizedPhoneNumber? PhoneNumber { get; set; }

    public async Task<List<string>> VerifyLoginInformation() {
        var errors = new List<string>();

        EmailAddress = Regex.Replace(EmailAddress?.Trim().ToLower() ?? string.Empty, Constants.MultiSpace, string.Empty);

        if (!EmailAddress.IsString() && PhoneNumber is null) errors.Add($"Neither {nameof(EmailAddress).Lucidify()} nor {nameof(PhoneNumber)} has been provided.");
        if (errors.Any()) return errors;

        if (EmailAddress.IsString()) errors = errors.Concat(EmailAddress.VerifyEmailAddress()).ToList();
        if (PhoneNumber is not null) errors = errors.Concat(await PhoneNumber.VerifyPhoneNumberData()).ToList();

        return errors;
    }
}

public class AuthenticationData: LoginInformation {

    public string Password { get; set; } = null!;
    
    public bool IsTrusted { get; set; }
    
    public DeviceInformation? DeviceInformation { get; set; }

    public async Task<List<string>> VerifyAuthenticationData() => await VerifyLoginInformation();
}