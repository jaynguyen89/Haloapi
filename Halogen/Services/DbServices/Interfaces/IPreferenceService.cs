using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

internal interface IPreferenceService {

    Task<string?> InsertNewPreference(Preference newPreference);
}