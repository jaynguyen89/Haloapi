using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;

namespace Halogen.Services.DbServices.Services; 

internal sealed class AccountService: ServiceBase, IAccountService {
    
    internal AccountService(
        ILogger<AccountService> logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }
    
    
}