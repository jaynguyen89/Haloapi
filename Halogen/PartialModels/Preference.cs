using Halogen.Bindings.ApiBindings;
using HelperLibrary;
using HelperLibrary.Shared;
using Newtonsoft.Json;

namespace Halogen.DbModels; 

public partial class Preference {

    public static Preference CreatePreferenceForNewAccount(bool useLongerId, string accountId) => new() {
        Id = StringHelpers.NewGuid(useLongerId),
        AccountId = accountId,
        ApplicationTheme = (byte)Enums.ApplicationTheme.Blue,
        ApplicationLanguage = (byte)Enums.Language.English,
        DateFormat = (byte)Enums.DateFormat.DDMMYYYYS,
        TimeFormat = (byte)Enums.TimeFormat.HHMMTTC,
        NumberFormat = (byte)Enums.NumberFormat.CommaForThousands,
        UnitSystem = (byte)Enums.UnitSystem.InternationalUnitSystem,
        Privacy = JsonConvert.SerializeObject(new PrivacyPreference {
            ProfilePreference = new PrivacyPreference.ProfilePolicy(),
            NamePreference = new PrivacyPreference.PrivacyPolicy {
                DataFormat = (byte)Enums.NameFormat.ShowFullName,
                Visibility = Enums.Visibility.VisibleToPublic
            },
            BirthPreference = new PrivacyPreference.PrivacyPolicy {
                DataFormat = (byte)Enums.BirthFormat.ShowMonthYearOnly,
                Visibility = Enums.Visibility.VisibleToPublic
            },
            CareerPreference = new PrivacyPreference.PrivacyPolicy {
                DataFormat = (byte)Enums.CareerFormat.ShowJobTitleOnly,
                Visibility = Enums.Visibility.VisibleToPublic,
            },
            PhoneNumberVisibility = Enums.Visibility.VisibleToSelf
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