using HelperLibrary.Shared.Logger;
using MediaLibrary.DbContexts;

namespace MediaLibrary.Services.DbServices;

public class ServiceBase {

    protected readonly ILoggerService _logger;
    protected readonly MediaLibraryDbContext _dbContext;
    protected readonly MediaRoutePath _routePath;

    internal ServiceBase(
        ILoggerService logger,
        MediaLibraryDbContext dbContext,
        MediaRoutePath routePath
    ) {
        _logger = logger;
        _dbContext = dbContext;
        _routePath = routePath;
    }
}
