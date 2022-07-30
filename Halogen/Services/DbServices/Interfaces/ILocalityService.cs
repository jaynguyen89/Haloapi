namespace Halogen.Services.DbServices.Interfaces; 

public interface ILocalityService {

    Task<string[]?> GetTelephoneCodes();
}