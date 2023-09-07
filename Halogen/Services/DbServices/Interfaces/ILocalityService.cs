using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

public interface ILocalityService {

    Task<string[]?> GetTelephoneCodes();

    Task<Locality[]?> GetLocalitiesForPublicData();
}