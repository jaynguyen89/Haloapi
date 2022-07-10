using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;

namespace Halogen.Services.DbServices.Services; 

internal sealed class ProfileService: ServiceBase, IProfileService {
    
    internal ProfileService(
        ILogger<ProfileService> logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }
}