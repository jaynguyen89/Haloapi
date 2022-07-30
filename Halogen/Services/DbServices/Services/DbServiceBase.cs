using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared.Logger;

namespace Halogen.Services.DbServices.Services; 

public class DbServiceBase: IDbServiceBase {

    protected readonly ILoggerService _logger;
    protected readonly HalogenDbContext _dbContext;

    protected DbServiceBase(
        ILoggerService logger,
        HalogenDbContext dbContext
    ) {
        _logger = logger;
        _dbContext = dbContext;
    }
}