using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;

namespace Halogen.Services.DbServices.Services; 

internal sealed class AuthenticationService: ServiceBase, IAuthenticationService {

    internal AuthenticationService(
        ILogger<AuthenticationService> logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }
    
    
}