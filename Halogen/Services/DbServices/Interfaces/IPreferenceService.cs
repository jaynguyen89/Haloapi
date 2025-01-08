using Halogen.Bindings.ViewModels;
using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

public interface IPreferenceService {

    /// <summary>
    /// To insert a Preference entity.
    /// </summary>
    /// <param name="newPreference">DbModels.Preference</param>
    /// <returns>string?</returns>
    Task<string?> InsertNewPreference(Preference newPreference);
    
    /// <summary>
    /// To get a Preference entity by accountId.
    /// </summary>
    /// <param name="accountId">string</param>
    /// <returns>PreferenceVM?</returns>
    Task<PreferenceVM?> GetPreferenceSettings(string accountId);
    
    /// <summary>
    /// To get a Privacy settings from a Preference entity by accountId.
    /// </summary>
    /// <param name="accountId">string</param>
    /// <returns>PrivacyVM?</returns>
    Task<PrivacyVM?> GetPrivacySettings(string accountId);
    
    /// <summary>
    /// To get a Preference entity.
    /// </summary>
    /// <param name="accountId">string</param>
    /// <returns>DbModels.Preference?</returns>
    Task<Preference?> GetPreference(string accountId);
    
    /// <summary>
    /// To update a Preference entity.
    /// </summary>
    /// <param name="preference">DbModels.Preference</param>
    /// <returns>bool?</returns>
    Task<bool?> UpdatePreference(Preference preference);
}
