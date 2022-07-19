namespace Halogen.Services.DbServices.Interfaces; 

internal interface ILocalityService {

    Task<string[]?> GetTelephoneCodes();
}