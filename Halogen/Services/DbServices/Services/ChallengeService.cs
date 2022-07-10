using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;

namespace Halogen.Services.DbServices.Services; 

internal sealed class ChallengeService: ServiceBase, IChallengeService {
    
    internal ChallengeService(
        ILogger<ChallengeService> logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }
}