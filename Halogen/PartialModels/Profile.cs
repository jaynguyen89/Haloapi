using Halogen.Bindings.ApiBindings;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace Halogen.DbModels; 

public partial class Profile {
    
    public static Profile CreateNewProfile(
        bool useLongerId,
        string accountId,
        bool registerByEmailAddress,
        int phoneTokenMinLength,
        int phoneTokenMaxLength,
        RegionalizedPhoneNumber? phoneNumber,
        RegistrationProfileData? profileData
    ) => new() {
        Id = StringHelpers.NewGuid(useLongerId),
        AccountId = accountId,
        PhoneNumber = !registerByEmailAddress ? JsonConvert.SerializeObject(phoneNumber) : default,
        PhoneNumberToken = !registerByEmailAddress
            ? StringHelpers.GenerateRandomString(NumberHelpers.GetRandomNumberInRangeInclusive(phoneTokenMinLength, phoneTokenMaxLength))
            : default,
        PhoneNumberTokenTimestamp = !registerByEmailAddress ? new DateTime() : default,
        Gender = profileData?.Gender ?? (byte) Enums.Gender.NotSpecified,
        GivenName = profileData?.GivenName,
        MiddleName = profileData?.MiddleName,
        LastName = profileData?.FamilyName,
        FullName = profileData?.FullName,
    };

    public string GetName() => NickName ?? FullName ?? $"{GivenName ?? string.Empty} {MiddleName ?? string.Empty} {LastName ?? string.Empty}".Trim();
}