using Halogen.Bindings.ViewModels;

namespace Halogen.Services.DbServices.Interfaces;

public interface IOccupationService {
    
    /// <summary>
    /// To get all Occupation entities.
    /// </summary>
    /// <returns>OccupationVM[]?</returns>
    Task<OccupationVM[]?> GetAllOccupations();
    
    /// <summary>
    /// To get all occupations for dropdown list.
    /// </summary>
    /// <returns>OccupationItemVM[]?</returns>
    Task<OccupationItemVM[]?> GetAllOccupationsAsList();
}