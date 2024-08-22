using System.Reflection;
using Halogen.DbContexts;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Services;
using Halogen.Services.AppServices.Services;
using Halogen.Services.HostedServices;
using HelperLibrary.Attributes;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Caching.Distributed;

namespace Halogen.Auxiliaries; 

[Todo("Refactor this class as described in comment below")]
/*
/// This class is registered to Autofac Builder as a Singleton dependency.
/// But each time we call GetService<T>(), a new instance of a service will be created, and it is bad.
/// So refactor this class to make method GetService<T>() return an existing instance of a service if any, otherwise, return a new instance of the service.
*/
public sealed class HaloServiceFactory: IHaloServiceFactory {
    
    private readonly HalogenDbContext _dbContext;
    private readonly ILoggerService _logger;
    private readonly IConfiguration _configuration;
    private readonly IDistributedCache _redisCache;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HaloServiceFactory(
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

    public T? GetService<T>(Enums.ServiceType serviceType) {
        try {
            return serviceType switch {
                Enums.ServiceType.DbService => (T)Activator.CreateInstance(typeof(T), _logger, _dbContext)!,
                Enums.ServiceType.AppService => (T)GetAppService<T>(),
                _ => (T)(IServiceBase)new HostedServiceBase(_logger),
            };
        }
        catch (ArgumentException e) {
            _logger.Log(new LoggerBinding<HaloServiceFactory> {
                Location = $"{nameof(GetService)}.{nameof(ArgumentException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (NotSupportedException e) {
            _logger.Log(new LoggerBinding<HaloServiceFactory> {
                Location = $"{nameof(GetService)}.{nameof(NotSupportedException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (TargetInvocationException e) {
            _logger.Log(new LoggerBinding<HaloServiceFactory> {
                Location = $"{nameof(GetService)}.{nameof(TargetInvocationException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (MethodAccessException e) {
            _logger.Log(new LoggerBinding<HaloServiceFactory> {
                Location = $"{nameof(GetService)}.{nameof(MethodAccessException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (MemberAccessException e) {
            _logger.Log(new LoggerBinding<HaloServiceFactory> {
                Location = $"{nameof(GetService)}.{nameof(MemberAccessException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    private IServiceBase GetAppService<T>() => typeof(T).Name switch {
        nameof(JwtService) => new JwtService(_logger, _configuration),
        nameof(RedisCache) => new RedisCache(_redisCache, _configuration, _logger),
        _ => new SessionService(_httpContextAccessor, _logger),
    };
}