using Halogen.Bindings.ApiBindings;
using HelperLibrary;
using HelperLibrary.Shared;
using Newtonsoft.Json;

namespace Halogen.DbModels; 

public partial class Preference {

    public static Preference CreatePreferenceForNewAccount(bool useLongerId, string accountId) => new() {
        Id = StringHelpers.NewGuid(useLongerId),
        AccountId = accountId,
        ApplicationTheme = (byte)Enums.ApplicationTheme.BLUE,
        ApplicationLanguage = (byte)Enums.Language.ENGLISH,
        DateFormat = (byte)Enums.DateFormat.DDMMYYYYS,
        TimeFormat = (byte)Enums.TimeFormat.HHMMTTC,
        NumberFormat = (byte)Enums.NumberFormat.COMMA_FOR_THOUSANDS,
        UnitSystem = (byte)Enums.UnitSystem.INTERNATIONAL_UNIT_SYSTEM,
        Privacy = JsonConvert.SerializeObject(new PrivacyPreference {
            ProfilePreference = new PrivacyPreference.ProfilePolicy(),
            NamePreference = new PrivacyPreference.PrivacyPolicy {
                DataFormat = (byte)Enums.NameFormat.SHOW_FULL_NAME,
                Visibility = Enums.Visibility.VISIBLE_TO_PUBLIC
            },
            BirthPreference = new PrivacyPreference.PrivacyPolicy {
                DataFormat = (byte)Enums.BirthFormat.SHOW_MONTH_YEAR_ONLY,
                Visibility = Enums.Visibility.VISIBLE_TO_PUBLIC
            },
            CareerPreference = new PrivacyPreference.PrivacyPolicy {
                DataFormat = (byte)Enums.CareerFormat.SHOW_JOB_TITLE_ONLY,
                Visibility = Enums.Visibility.VISIBLE_TO_PUBLIC,
            },
            PhoneNumberVisibility = Enums.Visibility.VISIBLE_TO_SELF
        })
    };
}

internal sealed class PrivacyPreference {

    public ProfilePolicy ProfilePreference { get; set; } = null!;

    public PrivacyPolicy NamePreference { get; set; } = null!;

    public PrivacyPolicy BirthPreference { get; set; } = null!;

    public PrivacyPolicy CareerPreference { get; set; } = null!;

    public Enums.Visibility PhoneNumberVisibility { get; set; }
    
    internal sealed class PrivacyPolicy: VisibilityPolicy, IPolicyWithDataFormat, IPolicyWithSingleTarget {

        public byte DataFormat { get; set; }

        public string? VisibleToTargetId { get; set; }

        public string? TargetTypeName { get; set; }
    }
    
    internal sealed class ProfilePolicy {
        
        public bool HiddenToSearchEngines { get; set; }
        
        public bool HiddenToStrangers { get; set; }
        
        public string? ReachableByTypeName { get; set; }
    }
}