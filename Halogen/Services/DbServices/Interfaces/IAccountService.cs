namespace Halogen.Services.DbServices.Interfaces; 

internal interface IAccountService {

    Task<bool?> IsEmailAvailableForNewAccount(string emailAddress);
}