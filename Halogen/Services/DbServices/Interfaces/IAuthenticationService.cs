using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

public interface IAuthenticationService {

    /// <summary>
    /// To insert an Account eneity.
    /// </summary>
    /// <param name="newAccount">Account</param>
    /// <returns>string</returns>
    Task<string?> InsertNewAccount(Account newAccount);
}