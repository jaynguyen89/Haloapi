using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;

namespace Halogen.Services.DbServices.Services; 

internal class ServiceBase: IServiceBase {

    private readonly ILogger<ServiceBase> _logger;
    protected readonly HalogenDbContext _dbContext;

    internal ServiceBase(
        ILogger<ServiceBase> logger,
        HalogenDbContext dbContext
    ) {
        _logger = logger;
        _dbContext = dbContext;
    }
}