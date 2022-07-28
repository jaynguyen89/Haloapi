using Halogen.DbContexts;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;

namespace Halogen.Services.DbServices.Services; 

internal sealed class ProfileService: DbServiceBase, IProfileService {
    
    internal ProfileService(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }

    public async Task<string?> InsertNewProfile(Profile newProfile) {
        _logger.Log(new LoggerBinding<ProfileService> { Location = nameof(InsertNewProfile) });
        await _dbContext.Profiles.AddAsync(newProfile);

        try {
            var result = await _dbContext.SaveChangesAsync();
            return result != 0 ? newProfile.Id : default;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<ProfileService> { Location = nameof(InsertNewProfile), Severity = Enums.LogSeverity.ERROR, Data = e });
            return default;
        }
    }
}