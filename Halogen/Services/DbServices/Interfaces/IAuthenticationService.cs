using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

public interface IAuthenticationService {

    Task<string?> InsertNewAccount(Account newAccount);
}