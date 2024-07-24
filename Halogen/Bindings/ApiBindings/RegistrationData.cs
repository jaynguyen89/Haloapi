using System.Text.RegularExpressions;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;

namespace Halogen.Bindings.ApiBindings; 

public sealed class RegistrationData: AuthenticationData {
    
    public string PasswordConfirm { get; set; } = null!;

    public string Username { get; set; } = null!;
    
    public RegistrationProfileData? ProfileData { get; set; }

    public async Task<Dictionary<string, List<string>>> VerifyRegistrationData(RegionalizedPhoneNumberHandler phoneNumberHandler) {
        var allErrors = await VerifyAuthenticationData(phoneNumberHandler);

        var usernameErrors = new List<string>();
        if (!Username.IsString()) usernameErrors.Add($"{nameof(Username)} must be provided.");
        if (usernameErrors.Any()) {
            allErrors.Add(nameof(Username), usernameErrors);
            return allErrors;
        }

        usernameErrors = Username.VerifyUsername();

        var passwordErrors = new List<string>();
        if (!Password.IsString() || !PasswordConfirm.IsString()) passwordErrors.Add($"Both {nameof(Password)} and {nameof(PasswordConfirm).Lucidify()} must be provided.");
        if (passwordErrors.Any()) {
            allErrors.MergeDataValidationErrors(
                new KeyValuePair<string, List<string>>(nameof(Username), usernameErrors),
                new KeyValuePair<string, List<string>>(nameof(Password), passwordErrors)
            );

            return allErrors;
        }
        
        passwordErrors = Password.VerifyPassword();
        if (!Password.Equals(PasswordConfirm)) passwordErrors.Add($"{nameof(Password)} and {nameof(PasswordConfirm).Lucidify()} do not match.");

        var profileDataErrors = new Dictionary<string, List<string>>();
        if (ProfileData is not null)
            profileDataErrors = ProfileData.VerifyRegistrationProfileData();

        allErrors.MergeDataValidationErrors(
            new KeyValuePair<string, List<string>>(nameof(Username), usernameErrors),
            new KeyValuePair<string, List<string>>(nameof(Password), passwordErrors)
        );

        return allErrors.Concat(profileDataErrors).ToDictionary(pair => pair.Key, pair => pair.Value);
    }
}
    
public sealed class RegistrationProfileData {
    
    public byte? Gender { get; set; }

    public string? GivenName { get; set; }
    
    public string? MiddleName { get; set; }

    public string? FamilyName { get; set; }

    public string? FullName { get; set; }

    public Dictionary<string, List<string>> VerifyRegistrationProfileData() {
        var allErrors = new Dictionary<string, List<string>>();
        if (Gender is > (byte) Enums.Gender.NotSpecified) allErrors.Add(nameof(Gender), [$"{nameof(Gender)} is not recognized."]);

        var givenNameErrors = new List<string>();
        if (GivenName.IsString()) {
            GivenName = Regex.Replace(GivenName!.Trim(), Constants.MultiSpace, Constants.MonoSpace);
            givenNameErrors = GivenName.VerifyName(nameof(GivenName));
        }
        
        var middleNameErrors = new List<string>();
        if (MiddleName.IsString()) {
            MiddleName = Regex.Replace(MiddleName!.Trim(), Constants.MultiSpace, Constants.MonoSpace);
            middleNameErrors = MiddleName.VerifyName(nameof(MiddleName));
        }
        
        var familyNameErrors = new List<string>();
        if (FamilyName.IsString()) {
            FamilyName = Regex.Replace(FamilyName!.Trim(), Constants.MultiSpace, Constants.MonoSpace);
            familyNameErrors = FamilyName.VerifyName(nameof(FamilyName));
        }

        FullName = FullName.IsString()
            ? Regex.Replace(FullName!.Trim(), Constants.MultiSpace, Constants.MonoSpace)
            : $"{GivenName} {MiddleName} {FamilyName}";

        allErrors.MergeDataValidationErrors(
            new KeyValuePair<string, List<string>>(nameof(GivenName), givenNameErrors),
            new KeyValuePair<string, List<string>>(nameof(MiddleName), middleNameErrors),
            new KeyValuePair<string, List<string>>(nameof(FamilyName), familyNameErrors)
        );

        return allErrors;
    }
}