﻿using Halogen.DbContexts;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;

namespace Halogen.Services.DbServices.Services; 

public sealed class PreferenceService: DbServiceBase, IPreferenceService {
    
    public PreferenceService(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }

    public async Task<string?> InsertNewPreference(Preference newPreference) {
        _logger.Log(new LoggerBinding<PreferenceService> { Location = nameof(InsertNewPreference) });
        await _dbContext.Preferences.AddAsync(newPreference);

        try {
            var result = await _dbContext.SaveChangesAsync();
            return result != 0 ? newPreference.Id : default;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<PreferenceService> {
                Location = $"{nameof(InsertNewPreference)}.{nameof(DbUpdateException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    public async Task<Preference?> GetPreferenceByAccountId(string accountId) {
        _logger.Log(new LoggerBinding<PreferenceService> { Location = nameof(GetPreferenceByAccountId) });
        try {
            return await _dbContext.Preferences.SingleOrDefaultAsync(x => x.AccountId.Equals(accountId));
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<PreferenceService> {
                Location = $"{nameof(GetPreferenceByAccountId)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (InvalidOperationException e) {
            _logger.Log(new LoggerBinding<PreferenceService> {
                Location = $"{nameof(GetPreferenceByAccountId)}.{nameof(InvalidOperationException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }
}