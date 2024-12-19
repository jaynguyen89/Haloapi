using Halogen.Bindings.ApiBindings;
using Halogen.Bindings.ViewModels;
using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

public interface IProfileService {

    Task<string?> InsertNewProfile(Profile newProfile);
    
    Task<bool?> IsPhoneNumberAvailableForNewAccount(string phoneNumber);
    
    Task<Profile?> GetProfileByAccountId(string accountId);
    
    Task<bool?> UpdateProfile(Profile profile);
    
    Task<Profile?> GetProfileByPhoneNumber(RegionalizedPhoneNumber phoneNumber);
    
    Task<Account?> GetAccountByPhoneNumber(RegionalizedPhoneNumber phoneNumber);
    
    Task<PhoneNumberCredentialVM?> GetPhoneNumberCredential(string accountId);
    
    Task<ProfileDetailsVM?> GetProfileDetails(string profileId);
    
    Task<bool?> IsProfileIdBelongedToAccount(string profileId, string accountId);
    
    Task<Profile?> GetProfile(string profileId);
}