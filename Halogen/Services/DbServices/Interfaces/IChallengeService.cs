using Halogen.Bindings.ViewModels;
using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

public interface IChallengeService {
    
    /// <summary>
    /// To get all the Challenge questions.
    /// </summary>
    /// <returns>ChallengeVM[]?</returns>
    Task<ChallengeVM[]?> GetChallengeQuestions();
    
    /// <summary>
    /// To get all ChallengeResponse entities by accountId.
    /// </summary>
    /// <param name="accountId">string</param>
    /// <returns>ChallengeResponseVM[]?</returns>
    Task<ChallengeResponseVM[]?> GetAllChallengeResponses(string accountId);
    
    /// <summary>
    /// To get a ChallengeResponse entity using composite keys.
    /// </summary>
    /// <param name="accountId">string</param>
    /// <param name="responseId">string</param>
    /// <returns>ChallengeResponse?</returns>
    Task<ChallengeResponse?> GetChallengeResponse(string accountId, string responseId);
    
    /// <summary>
    /// To update a ChallengeResponse entity.
    /// </summary>
    /// <param name="response">ChallengeResponse</param>
    /// <returns>bool?</returns>
    Task<bool?> UpdateChallengeResponse(ChallengeResponse response);
    
    /// <summary>
    /// To add multiple ChallengeResponse entities.
    /// </summary>
    /// <param name="challengeResponses">ChallengeResponse[]</param>
    /// <returns>bool?</returns>
    Task<bool?> AddChallengeResponsesMulti(ChallengeResponse[] challengeResponses);
    
    /// <summary>
    /// To delete a ChallengeResponse entity.
    /// </summary>
    /// <param name="response">ChallengeResponse</param>
    /// <returns>bool?</returns>
    Task<bool?> DeleteChallengeResponse(ChallengeResponse response);
}