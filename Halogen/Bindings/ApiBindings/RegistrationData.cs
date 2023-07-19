using HelperLibrary;
using HelperLibrary.Shared.Helpers;

namespace Halogen.Bindings.ApiBindings; 

public sealed class RegistrationData: AuthenticationData {
    
    public string PasswordConfirm { get; set; } = null!;

    public async Task<string[]> VerifyRegistrationData() {
        var errors = await VerifyAuthenticationData();
        
        if (!Password.IsString() || !PasswordConfirm.IsString()) errors.Add($"Both {nameof(Password)} and {nameof(PasswordConfirm).Lucidify()} must be provided.");
        if (errors.Any()) return errors.ToArray();
        
        if (!Password.Equals(PasswordConfirm)) errors.Add($"{nameof(Password)} and {nameof(PasswordConfirm).Lucidify()} do not matched.");

        var passwordErrors = Password.VerifyPassword();
        if (passwordErrors.Any()) errors = errors.Concat(passwordErrors).ToList();

        return errors.ToArray();
    }
}