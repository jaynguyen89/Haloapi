using Halogen.Bindings;
using Halogen.Services.AppServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Caching.Distributed;

namespace Halogen.Services.AppServices.Services;

public class CacheServiceFactory: AppServiceBase, ICacheServiceFactory {
    
    private readonly IConfiguration _configuration;
    private readonly IEcosystem _ecosystem;
    private readonly IDistributedCache _redisCache;

    private readonly bool _isMemoryCacheEnabled;
    private readonly string _activeServiceName;

    private readonly Lazy<Dictionary<string, object>> _services = new(() => new Dictionary<string, object>());

    public CacheServiceFactory() { }

    public CacheServiceFactory(
        IConfiguration configuration,
        ILoggerService logger,
        IEcosystem ecosystem,
        IDistributedCache redisCache
    ): base(logger) {
        _configuration = configuration;
        _ecosystem = ecosystem;
        _redisCache = redisCache;
        
        _isMemoryCacheEnabled = bool.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{ecosystem.GetEnvironment()}{Constants.Colon}{nameof(HalogenOptions.Local.MemoryCacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.MemoryCacheSettings.IsEnabled)}")!);
        _activeServiceName = _isMemoryCacheEnabled
            ? typeof(MemoryCache).AssemblyQualifiedName!
            : typeof(RedisCache).AssemblyQualifiedName!;
    }

    public ICacheService GetActiveCacheService() {
        if (_services.Value.ContainsKey(_activeServiceName)) return (ICacheService)_services.Value!.GetDictionaryValue(_activeServiceName)!;

        var serviceType = Type.GetType(_activeServiceName);
        if (serviceType == null) throw new ServiceNotFoundByNameException(_activeServiceName);

        var instance = _isMemoryCacheEnabled
            ? Activator.CreateInstance(serviceType, _configuration, _ecosystem, _logger)
            : Activator.CreateInstance(serviceType, _configuration, _ecosystem, _logger, _redisCache);
        
        var service = (ICacheService)instance!;
        _services.Value.Add(_activeServiceName, service);
        
        return service;
    }
}