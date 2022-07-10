using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;

namespace Halogen.Services.DbServices.Services; 

internal sealed class LocalityService: ServiceBase, ILocalityService {
    
    internal LocalityService(
        ILogger<LocalityService> logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }
}