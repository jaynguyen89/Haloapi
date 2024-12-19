using Halogen.Bindings.ViewModels;
using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

public interface IAccountService {

    Task<bool?> IsEmailAddressAvailableForNewAccount(string emailAddress);
    
    Task<Account?> GetAccountById(string accountId);
    
    Task<bool?> UpdateAccount(Account account);
    
    Task<Account?> GetAccountByEmailAddress(string emailAddress);
    
    Task<AuthenticatedUser?> GetInformationForAuthenticatedUser(string accountId);
    
    Task<EmailAddressCredentialVM?> GetEmailAddressCredential(string accountId);
}