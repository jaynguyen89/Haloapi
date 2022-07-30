using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared.Logger;

namespace Halogen.Services.DbServices.Services; 

public sealed class ChallengeService: DbServiceBase, IChallengeService {
    
    public ChallengeService(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }
}