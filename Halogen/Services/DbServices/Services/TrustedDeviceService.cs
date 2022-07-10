using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;

namespace Halogen.Services.DbServices.Services; 

internal sealed class TrustedDeviceService: ServiceBase, ITrustedDeviceService {
    
    internal TrustedDeviceService(
        ILogger<TrustedDeviceService> logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }
}