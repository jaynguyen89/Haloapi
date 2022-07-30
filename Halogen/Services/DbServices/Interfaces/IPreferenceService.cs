using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

public interface IPreferenceService {

    Task<string?> InsertNewPreference(Preference newPreference);
}