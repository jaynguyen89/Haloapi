using Halogen.Bindings.ViewModels;
using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

public interface IPreferenceService {

    Task<string?> InsertNewPreference(Preference newPreference);
    
    Task<PreferenceVM?> GetPreferenceSettings(string accountId);
    
    Task<PrivacyVM?> GetPrivacySettings(string accountId);
    
    Task<Preference?> GetPreference(string accountId);
    
    Task<bool?> UpdatePreference(Preference preference);
}
