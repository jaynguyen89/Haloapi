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
        DataFormat = JsonConvert.SerializeObject(new[] {
            new DataFormat {
                DtType = Bindings.ApiBindings.DataFormat.DataType.Date,
                Format = (byte)Enums.DateFormat.DDMMYYYYS,
            },
            new DataFormat {
                DtType = Bindings.ApiBindings.DataFormat.DataType.Time,
                Format = (byte)Enums.TimeFormat.HHMMTTC,
            },
            new DataFormat {
                DtType = Bindings.ApiBindings.DataFormat.DataType.Number,
                Format = (byte)Enums.NumberFormat.CommaForThousands,
            },
            new DataFormat {
                DtType = Bindings.ApiBindings.DataFormat.DataType.UnitSystem,
                Format = (byte)Enums.UnitSystem.InternationalUnitSystem,
            },
        }),
        Privacy = JsonConvert.SerializeObject(new PrivacyPreference {
            ProfilePreference = new ProfilePolicy(),
            NamePreference = new PrivacyPolicy {
                DataFormat = (byte)Enums.NameFormat.ShowFullName,
                Visibility = Enums.Visibility.VisibleToPublic,
            },
            BirthPreference = new PrivacyPolicy {
                DataFormat = (byte)Enums.BirthFormat.ShowMonthYearOnly,
                Visibility = Enums.Visibility.VisibleToPublic,
            },
            CareerPreference = new PrivacyPolicy {
                DataFormat = (byte)Enums.CareerFormat.ShowJobTitleOnly,
                Visibility = Enums.Visibility.VisibleToPublic,
            },
            PhoneNumberPreference = new PrivacyPolicy {
                DataFormat = (byte)Enums.PhoneNumberFormat.WithRegionCode_SpaceDelimited,
                Visibility = Enums.Visibility.VisibleToSelf,
            },
            SecurityPreference = new SecurityPolicy()
        })
    };
}