﻿using Halogen.Bindings.ApiBindings;
using Halogen.Bindings.ViewModels;
using Halogen.DbContexts;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Preference = Halogen.Bindings.ServiceBindings.Preference;

namespace Halogen.Services.DbServices.Services; 

public sealed class ChallengeService: DbServiceBase, IChallengeService {

    public ChallengeService(
        ILoggerService logger,
        HalogenDbContext dbContext,
        HttpContext httpContext
    ): base(logger, dbContext, httpContext) { }

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
            var storedPreferences = _httpContext!.Session.GetString(Enums.SessionKey.Preference.GetValue()!);
            if (!storedPreferences.IsString()) return default;

            var preferences = JsonConvert.DeserializeObject<Preference>(storedPreferences!);
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

    public async Task<ChallengeResponse?> GetChallengeResponse(string accountId, string responseId) {
        _logger.Log(new LoggerBinding<ChallengeService> { Location = nameof(GetChallengeResponse) });

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

    public async Task<bool?> AddChallengeResponsesMulti(ChallengeResponse[] challengeResponses) {
        _logger.Log(new LoggerBinding<ChallengeService> { Location = nameof(AddChallengeResponsesMulti) });

        try {
            await _dbContext.ChallengeResponses.AddRangeAsync(challengeResponses);
            var result = await _dbContext.SaveChangesAsync();
            return result == challengeResponses.Length;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<ChallengeService> {
                Location = $"{nameof(AddChallengeResponsesMulti)}.{nameof(DbUpdateException)}",
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