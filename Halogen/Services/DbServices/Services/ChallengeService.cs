using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings;
using Halogen.Bindings.ApiBindings;
using Halogen.Bindings.ViewModels;
using Halogen.DbContexts;
using Halogen.DbModels;
using Halogen.Services.AppServices.Interfaces;
using Halogen.Services.AppServices.Services;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;
using Preference = Halogen.Bindings.ServiceBindings.Preference;

namespace Halogen.Services.DbServices.Services; 

public sealed class ChallengeService: DbServiceBase, IChallengeService {
    
    private readonly ISessionService _sessionService;

    public ChallengeService(
        ILoggerService logger,
        HalogenDbContext dbContext,
        IHaloServiceFactory haloServiceFactory 
    ): base(logger, dbContext) {
        _sessionService = haloServiceFactory.GetService<SessionService>(Enums.ServiceType.AppService) ?? throw new HaloArgumentNullException<ChallengeService>(nameof(SessionService));
    }

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

        try {
            var preferences = _sessionService.Get<Preference>(Enums.SessionKey.Preference.GetValue()!);
            var (dateFormat, timeFormat) = GetPreferenceDateTimeFormats(preferences);

            return await _dbContext.ChallengeResponses
                .Where(e => e.AccountId == accountId)
                .Join(
                    _dbContext.Challenges,
                    challengeResponse => challengeResponse.ChallengeId,
                    challenge => challenge.Id,
                    (challengeResponse, challenge) => new {
                        ChallengeResponse = challengeResponse,
                        Challenge = challenge,
                    }
                )
                .Select(e => new ChallengeResponseVM {
                    Id = e.ChallengeResponse.Id,
                    Response = e.ChallengeResponse.Response,
                    UpdatedOn = e.ChallengeResponse.UpdatedOn.Format(dateFormat, timeFormat),
                    Challenge = new ChallengeVM {
                        Id = e.Challenge.Id,
                        Question = e.Challenge.Question,
                    },
                })
                .ToArrayAsync();
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<ChallengeService> {
                Location = $"{nameof(GetAllChallengeResponses)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }

        Tuple<Enums.DateFormat, Enums.TimeFormat> GetPreferenceDateTimeFormats(Preference? preference) {
            if (preference is null) return new Tuple<Enums.DateFormat, Enums.TimeFormat>(Enums.DateFormat.DDMMMYYYY, Enums.TimeFormat.HHMMTTC);

            var dateFormat = preference.DataFormats.First(format => format.DtType == DataFormat.DataType.Date).Format;
            var timeFormat = preference.DataFormats.First(format => format.DtType == DataFormat.DataType.Time).Format;
            
            return new Tuple<Enums.DateFormat, Enums.TimeFormat>((Enums.DateFormat)dateFormat, (Enums.TimeFormat)timeFormat);
        }
    }

    public async Task<ChallengeResponse?> GetChalengeResponse(string accountId, string responseId) {
        _logger.Log(new LoggerBinding<ChallengeService> { Location = nameof(GetChalengeResponse) });

        var challengeResponse = await _dbContext.ChallengeResponses.FindAsync(responseId);
        if (challengeResponse is null) return default;
        return challengeResponse.AccountId == accountId ? challengeResponse : default;
    }

    public async Task<bool?> UpdateChallengeResponse(ChallengeResponse response) {
        _logger.Log(new LoggerBinding<ChallengeService> { Location = nameof(UpdateChallengeResponse) });

        try {
            _dbContext.Update(response);
            var result = await _dbContext.SaveChangesAsync();
            return result == 1;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<ChallengeService> {
                Location = $"{nameof(UpdateChallengeResponse)}.{nameof(DbUpdateException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<bool?> AddChallengeReponsesMulti(ChallengeResponse[] challengeResponses) {
        _logger.Log(new LoggerBinding<ChallengeService> { Location = nameof(AddChallengeReponsesMulti) });

        try {
            await _dbContext.ChallengeResponses.AddRangeAsync(challengeResponses);
            var result = await _dbContext.SaveChangesAsync();
            return result == challengeResponses.Length;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<ChallengeService> {
                Location = $"{nameof(AddChallengeReponsesMulti)}.{nameof(DbUpdateException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<bool?> DeleteChallengeResponse(ChallengeResponse response) {
        _logger.Log(new LoggerBinding<ChallengeService> { Location = nameof(DeleteChallengeResponse) });

        try {
            _dbContext.ChallengeResponses.Remove(response);
            var result = await _dbContext.SaveChangesAsync();
            return result == 1;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<ChallengeService> {
                Location = $"{nameof(DeleteChallengeResponse)}.{nameof(DbUpdateException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }
}