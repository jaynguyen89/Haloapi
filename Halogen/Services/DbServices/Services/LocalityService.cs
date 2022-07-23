using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;

namespace Halogen.Services.DbServices.Services; 

internal sealed class LocalityService: DbServiceBase, ILocalityService {
    
    internal LocalityService(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }

    public async Task<string[]?> GetTelephoneCodes() {
        _logger.Log(new LoggerBinding<LocalityService> { Location = nameof(GetTelephoneCodes) });
        return await _dbContext.Localities.Select(locality => locality.TelephoneCode).ToArrayAsync();
    }
}
