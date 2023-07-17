using Halogen.DbContexts;
using Halogen.FactoriesAndMiddlewares.Interfaces;
using Halogen.Services;
using Halogen.Services.AppServices.Services;
using Halogen.Services.HostedServices;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Caching.Distributed;

namespace Halogen.FactoriesAndMiddlewares; 

public sealed class ServiceFactory: IServiceFactory {
    
    private readonly HalogenDbContext _dbContext;
    private readonly ILoggerService _logger;
    private readonly IConfiguration _configuration;
    private readonly IDistributedCache _redisCache;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServiceFactory(
        HalogenDbContext dbContext,
        ILoggerService logger,
        IConfiguration configuration,
        IDistributedCache redisCache,
        IHttpContextAccessor httpContextAccessor
    ) {
        _dbContext = dbContext;
        _logger = logger;
        _configuration = configuration;
        _redisCache = redisCache;
        _httpContextAccessor = httpContextAccessor;
    }

    public IServiceBase GetService<T>(Enums.ServiceType serviceType) => (serviceType switch {
        Enums.ServiceType.DbService => (IServiceBase) Activator.CreateInstance(typeof(T), _logger, _dbContext)!,
        Enums.ServiceType.AppService => GetAppService<T>(),
        _ => new HostedServiceBase(_logger),
    });

    private IServiceBase GetAppService<T>() => typeof(T).Name switch {
        nameof(JwtService) => new JwtService(_logger, _configuration),
        nameof(RedisCache) => new RedisCache(_redisCache, _configuration, _logger),
        _ => new SessionService(_httpContextAccessor, _logger),
    };
}