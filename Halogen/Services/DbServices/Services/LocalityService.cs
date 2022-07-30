using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;

namespace Halogen.Services.DbServices.Services; 

public sealed class LocalityService: DbServiceBase, ILocalityService {
    
    public LocalityService(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }

    public async Task<string[]?> GetTelephoneCodes() {
        _logger.Log(new LoggerBinding<LocalityService> { Location = nameof(GetTelephoneCodes) });
        return await _dbContext.Localities.Select(locality => locality.TelephoneCode).ToArrayAsync();
    }
}
