using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;

namespace Halogen.Services.DbServices.Services; 

internal sealed class RoleService: ServiceBase, IRoleService {
    
    internal RoleService(
        ILogger<RoleService> logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }
    
    
}