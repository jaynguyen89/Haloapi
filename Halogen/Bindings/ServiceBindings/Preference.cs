using Halogen.Bindings.ApiBindings;
using HelperLibrary.Shared;
using Newtonsoft.Json;

namespace Halogen.Bindings.ServiceBindings;

public sealed class Preference {
    
    public Enums.ApplicationTheme AppTheme { get; set; }
    
    public Enums.Language AppLanguage { get; set; }

    public DataFormat[] DataFormats { get; set; } = null!;

    public PrivacyPreference Preferences { get; set; } = null!;

    public static implicit operator Preference(DbModels.Preference preference) => new() {
        AppTheme = (Enums.ApplicationTheme)preference.ApplicationTheme,
        AppLanguage = (Enums.Language)preference.ApplicationLanguage,
        DataFormats = JsonConvert.DeserializeObject<DataFormat[]>(preference.DataFormat!)!,
        Preferences = JsonConvert.DeserializeObject<PrivacyPreference>(preference.Privacy!)!,
    };
}