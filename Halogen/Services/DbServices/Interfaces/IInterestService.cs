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

    /// <summary>
    /// To get all Interests for dropdown list.
    /// </summary>
    /// <returns>InterestListVM[]?</returns>
    Task<InterestItemVM[]?> GetAllInterestsAsList();

    /// <summary>
    /// To get all Interests for user.
    /// </summary>
    /// <param name="profileId">string</param>
    /// <returns>InterestVM[]?</returns>
    Task<InterestVM[]?> GetProfileInterests(string profileId);
}