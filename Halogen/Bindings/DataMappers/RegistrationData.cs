using System.Text.RegularExpressions;
using Halogen.Bindings.ApiBindings;
using HelperLibrary;
using HelperLibrary.Shared;

namespace Halogen.Bindings.DataMappers; 

public sealed class RegistrationData {

    public string? EmailAddress { get; set; }
    
    public RegionalizedPhoneNumber? PhoneNumber { get; set; }
    
    public string Password { get; set; } = null!;
    
    public string PasswordConfirm { get; set; } = null!;

    public async Task<string[]> VerifyRegistrationData() {
        var errors = new List<string>();

        EmailAddress = Regex.Replace(EmailAddress?.Trim().ToLower() ?? string.Empty, Constants.MultiSpace, string.Empty);

        if (!EmailAddress.IsString() && PhoneNumber is null) errors.Add($"Neither {nameof(EmailAddress).ToHumanStyled()} nor {nameof(PhoneNumber)} has been provided.");
        if (!Password.IsString() || !PasswordConfirm.IsString()) errors.Add($"Both {nameof(Password)} and {nameof(PasswordConfirm).ToHumanStyled()} must be provided.");
        if (errors.Any()) return errors.ToArray();

        if (EmailAddress.IsString()) errors = errors.Concat(EmailAddress.VerifyEmailAddress()).ToList();
        if (PhoneNumber is not null) errors = errors.Concat(await PhoneNumber.VerifyPhoneNumberData()).ToList();
        if (!Password.Equals(PasswordConfirm)) errors.Add($"{nameof(Password)} and {nameof(PasswordConfirm).ToHumanStyled()} do not matched.");

        return errors.ToArray();
    }
}