using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;

namespace Halogen.Services.DbServices.Services; 

internal sealed class AddressService: ServiceBase, IAddressService {
    
    internal AddressService(
        ILogger<AddressService> logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }
}