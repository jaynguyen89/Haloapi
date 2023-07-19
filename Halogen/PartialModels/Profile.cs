using Halogen.Bindings.ApiBindings;
using HelperLibrary;
using HelperLibrary.Shared.Helpers;
using Newtonsoft.Json;

namespace Halogen.DbModels; 

public partial class Profile {
    public static Profile CreateNewProfile(bool useLongerId, string accountId, bool registerByEmailAddress, int phoneTokenMinLength, int phoneTokenMaxLength, RegionalizedPhoneNumber? phoneNumber) => new() {
        Id = StringHelpers.NewGuid(useLongerId),
        AccountId = accountId,
        PhoneNumber = !registerByEmailAddress ? JsonConvert.SerializeObject(phoneNumber) : default,
        PhoneNumberToken = !registerByEmailAddress
            ? StringHelpers.GenerateRandomString(NumberHelpers.GetRandomNumberInRangeInclusive(phoneTokenMinLength, phoneTokenMaxLength))
            : default,
        PhoneNumberTokenTimestamp = !registerByEmailAddress ? new DateTime() : default
    };
}