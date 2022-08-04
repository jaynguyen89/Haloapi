using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

public interface IAccountService {

    Task<bool?> IsEmailAddressAvailableForNewAccount(string emailAddress);
    
    Task<Account?> GetAccountById(string accountId);
    
    Task<bool?> UpdateAccount(Account account);
}