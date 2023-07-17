using Halogen.Bindings;
using Halogen.Bindings.ServiceBindings;
using Halogen.Services.AppServices.Interfaces;
using HelperLibrary;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Caching.Distributed;

namespace Halogen.Services.AppServices.Services; 

internal sealed class RedisCache: AppServiceBase, ICacheService {

    public bool IsEnabled { get; set; }
    public int SlidingExpiration { get; set; }
    public int AbsoluteExpiration { get; set; }

    private readonly IDistributedCache _redisCache;

    public RedisCache(
        IDistributedCache redisCache,
        IConfiguration configuration,
        ILoggerService logger
    ): base(logger) {
        _redisCache = redisCache;
        
        var environment = configuration.GetValue<string>($"{nameof(Halogen)}Environment");
        var (isEnabled, slidingExpiration, absoluteExpiration) = (
            bool.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.CacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.CacheSettings.IsEnabled)}")),
            int.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.CacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.CacheSettings.SlidingExpiration)}")),
            int.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.CacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.CacheSettings.AbsoluteExpiration)}"))
        );

        IsEnabled = isEnabled;
        SlidingExpiration = slidingExpiration;
        AbsoluteExpiration = absoluteExpiration;
    }

    public async Task InsertCacheEntry(CacheEntry entry) {
        _logger.Log(new LoggerBinding<RedisCache> { Location = nameof(InsertCacheEntry) });
        if (!IsEnabled) return;

        await _redisCache.SetAsync(
            entry.Key,
            entry.Value.EncodeDataUtf8(),
            new DistributedCacheEntryOptions {
                SlidingExpiration = TimeSpan.FromSeconds(SlidingExpiration),
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(AbsoluteExpiration)
            }
        );
    }

    public async Task<T?> GetCacheEntry<T>(string key) {
        _logger.Log(new LoggerBinding<RedisCache> { Location = nameof(GetCacheEntry) });
        if (!IsEnabled) return default;

        try {
            var cachedValue = await _redisCache.GetAsync(key);
            if (cachedValue is null) return default;

            await _redisCache.RefreshAsync(key);
            return cachedValue.DecodeUtf8<T>();
        }
        catch (Exception) {
            return default;
        }
    }

    public async Task RemoveCacheEntry(string key) {
        _logger.Log(new LoggerBinding<RedisCache> { Location = nameof(RemoveCacheEntry) });
        if (!IsEnabled) return;
        await _redisCache.RemoveAsync(key);
    }
}