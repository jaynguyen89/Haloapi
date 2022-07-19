using System.Text.RegularExpressions;
using HelperLibrary;
using HelperLibrary.Shared;

namespace Halogen.Bindings.ApiBindings; 

internal sealed class RegistrationData {

    public string? EmailAddress { get; set; }
    
    public PhoneNumberData? PhoneNumber { get; set; }
    
    public string Password { get; set; } = null!;
    
    public string PasswordConfirm { get; set; } = null!;
    

    internal async Task<string[]> VerifyRegistrationData() {
        var errors = new List<string>();

        EmailAddress = Regex.Replace(EmailAddress?.Trim() ?? string.Empty, Constants.MultiSpace, string.Empty);

        if (!EmailAddress.IsString() && PhoneNumber is null) errors.Add($"Neither {nameof(EmailAddress).ToHumanStyled()} nor {nameof(PhoneNumber)} has been provided.");
        if (!Password.IsString() || !PasswordConfirm.IsString()) errors.Add($"Both {nameof(Password)} and {nameof(PasswordConfirm).ToHumanStyled()} must be provided.");
        if (errors.Any()) return errors.ToArray();

        if (EmailAddress.IsString()) errors = errors.Concat(EmailAddress.VerifyEmailAddress()).ToList();
        if (PhoneNumber is not null) errors = errors.Concat(await PhoneNumber.VerifyPhoneNumberData()).ToList();
        if (!Password.Equals(PasswordConfirm)) errors.Add($"{nameof(Password)} and {nameof(PasswordConfirm).ToHumanStyled()} do not matched.");

        return errors.ToArray();
    }
}