using Halogen.Bindings.ViewModels;
using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

public interface IAccountService {

    /// <summary>
    /// To check if the Email Address is redundant with another Account's.
    /// </summary>
    /// <param name="emailAddress">string</param>
    /// <returns>bool?</returns>
    Task<bool?> IsEmailAddressAvailableForNewAccount(string emailAddress);
    
    /// <summary>
    /// To get the Account entity by accountId.
    /// </summary>
    /// <param name="accountId">string</param>
    /// <returns>Account?</returns>
    Task<Account?> GetAccountById(string accountId);
    
    /// <summary>
    /// To update the Account entity.
    /// </summary>
    /// <param name="account">Account</param>
    /// <returns>bool?</returns>
    Task<bool?> UpdateAccount(Account account);
    
    /// <summary>
    /// To get the Account by Email Address.
    /// </summary>
    /// <param name="emailAddress">string</param>
    /// <returns>Account?</returns>
    Task<Account?> GetAccountByEmailAddress(string emailAddress);
    
    /// <summary>
    /// To get the crucial data for Authenticated User after login.
    /// </summary>
    /// <param name="accountId">string</param>
    /// <returns>AuthenticatedUser?</returns>
    Task<AuthenticatedUser?> GetInformationForAuthenticatedUser(string accountId);
    
    /// <summary>
    /// To get the credential information corresponding to Email Address.
    /// </summary>
    /// <param name="accountId">string</param>
    /// <returns>EmailAddressCredentialVM?</returns>
    Task<EmailAddressCredentialVM?> GetEmailAddressCredential(string accountId);
}