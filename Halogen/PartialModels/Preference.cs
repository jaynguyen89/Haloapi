using Halogen.Bindings.ApiBindings;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using Newtonsoft.Json;

namespace Halogen.DbModels; 

public partial class Preference {

    public static Preference CreatePreferenceForNewAccount(bool useLongerId, string accountId) => new() {
        Id = StringHelpers.NewGuid(useLongerId),
        AccountId = accountId,
        ApplicationTheme = (byte)Enums.ApplicationTheme.Day,
        ApplicationLanguage = (byte)Enums.Language.English,
        DateFormat = (byte)Enums.DateFormat.DDMMYYYYS,
        TimeFormat = (byte)Enums.TimeFormat.HHMMTTC,
        NumberFormat = (byte)Enums.NumberFormat.CommaForThousands,
        UnitSystem = (byte)Enums.UnitSystem.InternationalUnitSystem,
        Privacy = JsonConvert.SerializeObject(new PrivacyPreference {
            ProfilePreference = new ProfilePolicy(),
            NamePreference = new PrivacyPolicy {
                DataFormat = (byte)Enums.NameFormat.ShowFullName,
                Visibility = Enums.Visibility.VisibleToPublic
            },
            BirthPreference = new PrivacyPolicy {
                DataFormat = (byte)Enums.BirthFormat.ShowMonthYearOnly,
                Visibility = Enums.Visibility.VisibleToPublic
            },
            CareerPreference = new PrivacyPolicy {
                DataFormat = (byte)Enums.CareerFormat.ShowJobTitleOnly,
                Visibility = Enums.Visibility.VisibleToPublic,
            },
            PhoneNumberVisibility = Enums.Visibility.VisibleToSelf,
            SecurityPreference = new SecurityPolicy()
        })
    };
}