using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

internal interface IAuthenticationService {

    Task<string?> InsertNewAccount(Account newAccount);
}