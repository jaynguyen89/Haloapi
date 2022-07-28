using Halogen.DbContexts;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;

namespace Halogen.Services.DbServices.Services; 

internal sealed class PreferenceService: DbServiceBase, IPreferenceService {
    
    internal PreferenceService(
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
            _logger.Log(new LoggerBinding<PreferenceService> { Location = nameof(InsertNewPreference), Severity = Enums.LogSeverity.ERROR, Data = e });
            return default;
        }
    }
}