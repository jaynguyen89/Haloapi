using Halogen.Auxiliaries.Interfaces;
using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared.Logger;

namespace Halogen.Services.DbServices.Services; 

public class DbServiceBase: ServiceBase, IDbServiceBase {

    protected readonly HalogenDbContext _dbContext;
    protected readonly IHaloServiceFactory _haloServiceFactory;

    protected DbServiceBase() { }

    protected DbServiceBase(
        ILoggerService logger,
        HalogenDbContext dbContext,
        IHaloServiceFactory haloServiceFactory
    ): base(logger) {
        _dbContext = dbContext;
        _haloServiceFactory = haloServiceFactory;
    }
}