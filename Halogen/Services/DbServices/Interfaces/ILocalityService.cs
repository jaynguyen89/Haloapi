using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

public interface ILocalityService {

    /// <summary>
    /// To get the telephone codes of all Locality entities.
    /// </summary>
    /// <returns>string[]?</returns>
    Task<string[]?> GetTelephoneCodes();

    /// <summary>
    /// To get all Locality entities.
    /// </summary>
    /// <returns>Locality[]?</returns>
    Task<Locality[]?> GetLocalitiesForPublicData();
}