namespace Halogen.Services.DbServices.Interfaces; 

public interface IAccountService {

    Task<bool?> IsEmailAddressAvailableForNewAccount(string emailAddress);
}