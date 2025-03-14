using Halogen.Bindings.ViewModels;
using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

public interface ILocalityService {

    /// <summary>
    /// To get the telephone codes of all Locality entities.
    /// </summary>
    /// <returns>string[]?</returns>
    Task<string[]?> GetTelephoneCodes();
    
    Task<CountryVM[]?> GetCountriesAsPublicData();

    /// <summary>
    /// To get all Locality entities as CountryVM.
    /// </summary>
    /// <returns>CountryVM[]?</returns>
    Task<CountryVM[]?> GetCountries(bool minimal = true);

    Task<Locality?> GetCountryById(string countryId);
    
    Task<LocalityVM[]?> GetLocalities();
}