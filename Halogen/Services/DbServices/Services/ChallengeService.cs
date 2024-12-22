using Halogen.Bindings.ViewModels;
using Halogen.DbContexts;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;

namespace Halogen.Services.DbServices.Services; 

public sealed class ChallengeService: DbServiceBase, IChallengeService {
    
    public ChallengeService(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }

    public async Task<ChallengeVM[]?> GetChallengeQuestions() {
        _logger.Log(new LoggerBinding<ChallengeService> { Location = nameof(GetChallengeQuestions) });

        try {
            return await _dbContext.Challenges
                .Select(challenge => new ChallengeVM {
                    Id = challenge.Id,
                    Question = challenge.Question,
                })
                .ToArrayAsync();
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<ChallengeService> {
                Location = $"{nameof(GetChallengeQuestions)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<ChallengeResponseVM[]?> GetAllChallengeResponses(string accountId) {
        _logger.Log(new LoggerBinding<ChallengeService> { Location = nameof(GetAllChallengeResponses) });
    }

    public async Task<ChallengeResponse?> GetChalengeResponse(string accountId, string responseId) {
        _logger.Log(new LoggerBinding<ChallengeService> { Location = nameof(GetChalengeResponse) });
    }

    public async Task<bool?> UpdateChallengeResponse(ChallengeResponse response) {
        _logger.Log(new LoggerBinding<ChallengeService> { Location = nameof(UpdateChallengeResponse) });
    }

    public async Task<bool?> AddChallengeReponsesMulti(ChallengeResponse[] challengeResponses) {
        _logger.Log(new LoggerBinding<ChallengeService> { Location = nameof(AddChallengeReponsesMulti) });
    }

    public async Task<bool?> DeleteChallengeResponse(ChallengeResponse response) {
        _logger.Log(new LoggerBinding<ChallengeService> { Location = nameof(DeleteChallengeResponse) });
    }
}