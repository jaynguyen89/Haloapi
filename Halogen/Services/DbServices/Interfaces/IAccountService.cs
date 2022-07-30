namespace Halogen.Services.DbServices.Interfaces; 

public interface IAccountService {

    Task<bool?> IsEmailAvailableForNewAccount(string emailAddress);
}