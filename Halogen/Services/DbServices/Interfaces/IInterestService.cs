using Halogen.Bindings.ViewModels;

namespace Halogen.Services.DbServices.Interfaces;

public interface IInterestService {
    
    /// <summary>
    /// To get the Id of all Interest entities.
    /// </summary>
    /// <returns>string[]?</returns>
    Task<string[]?> GetAllIds();

    /// <summary>
    /// To get all Interest entities.
    /// </summary>
    /// <returns>InterestVM[]?</returns>
    Task<InterestVM[]?> GetAllInterests();
}