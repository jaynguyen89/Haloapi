using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;

namespace Halogen.Services.DbServices.Services; 

internal sealed class PreferenceService: ServiceBase, IPreferenceService {
    
    internal PreferenceService(
        ILogger<PreferenceService> logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }
}