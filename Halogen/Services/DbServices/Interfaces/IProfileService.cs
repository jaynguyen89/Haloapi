using Halogen.Bindings.ApiBindings;
using Halogen.Bindings.ViewModels;
using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

public interface IProfileService {

    /// <summary>
    /// To insert a Profile entity.
    /// </summary>
    /// <param name="newProfile">DbModels.Profile</param>
    /// <returns>string?</returns>
    Task<string?> InsertNewProfile(Profile newProfile);
    
    /// <summary>
    /// To check if a Phone Number is redundant with another Account's.
    /// </summary>
    /// <param name="phoneNumber">string</param>
    /// <returns>bool?</returns>
    Task<bool?> IsPhoneNumberAvailableForNewAccount(string phoneNumber);
    
    /// <summary>
    /// To get a Profile entity by accountId.
    /// </summary>
    /// <param name="accountId">string</param>
    /// <returns>DbModels.Profile?</returns>
    Task<Profile?> GetProfileByAccountId(string accountId);
    
    /// <summary>
    /// To update a Profile entity.
    /// </summary>
    /// <param name="profile">DbModels.Profile</param>
    /// <returns>bool?</returns>
    Task<bool?> UpdateProfile(Profile profile);
    
    /// <summary>
    /// To get a Profile entity using the Phone Number.
    /// </summary>
    /// <param name="phoneNumber">RegionalizedPhoneNumber</param>
    /// <returns>DbModels.Profile?</returns>
    Task<Profile?> GetProfileByPhoneNumber(RegionalizedPhoneNumber phoneNumber);
    
    /// <summary>
    /// To get an Account entity using the Phone Number.
    /// </summary>
    /// <param name="phoneNumber">RegionalizedPhoneNumber</param>
    /// <returns>Account?</returns>
    Task<Account?> GetAccountByPhoneNumber(RegionalizedPhoneNumber phoneNumber);
    
    /// <summary>
    /// To get the credential information relating to the Phone Number for an accountId.
    /// </summary>
    /// <param name="accountId">string</param>
    /// <returns>PhoneNumberCredentialVM?</returns>
    Task<PhoneNumberCredentialVM?> GetPhoneNumberCredential(string accountId);
    
    /// <summary>
    /// To get the basic Profile details by profileId.
    /// </summary>
    /// <param name="profileId">string</param>
    /// <returns>ProfileDetailsVM?</returns>
    Task<ProfileDetailsVM?> GetProfileDetails(string profileId);
    
    /// <summary>
    /// To check if the profileId and accountId are related, associating to the same user.
    /// </summary>
    /// <param name="profileId">string</param>
    /// <param name="accountId">string</param>
    /// <returns>bool?</returns>
    Task<bool?> IsProfileIdBelongedToAccount(string profileId, string accountId);
    
    /// <summary>
    /// To get the Profile entity using profileId.
    /// </summary>
    /// <param name="profileId">string</param>
    /// <returns>DbModels.Profile?</returns>
    Task<Profile?> GetProfile(string profileId);
}