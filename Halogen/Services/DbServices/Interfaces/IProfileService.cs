using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

public interface IProfileService {

    Task<string?> InsertNewProfile(Profile newProfile);
    
    Task<bool?> IsPhoneNumberAvailableForNewAccount(string phoneNumber);
    
    Task<Profile?> GetProfileByAccountId(string accountId);
    
    Task<bool?> UpdateProfile(Profile profile);
}