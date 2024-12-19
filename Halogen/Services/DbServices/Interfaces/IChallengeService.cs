using Halogen.Bindings.ViewModels;
using Halogen.DbModels;

namespace Halogen.Services.DbServices.Interfaces; 

public interface IChallengeService {
    
    Task<ChallengeVM[]?> GetChallengeQuestions();
    
    Task<ChallengeResponseVM[]?> GetAllChallengeResponses(string accountId);
    
    Task<ChallengeResponse?> GetChalengeResponse(string accountId, string responseId);
    
    Task<bool?> UpdateChallengeResponse(ChallengeResponse response);
    
    Task<bool?> AddChallengeReponsesMulti(ChallengeResponse[] challengeResponses);
    
    Task<bool?> DeleteChallengeResponse(ChallengeResponse response);
}