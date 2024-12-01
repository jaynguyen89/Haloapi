using Halogen.Bindings;
using Halogen.Bindings.ServiceBindings;
using Halogen.Services.AppServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Caching.Distributed;

namespace Halogen.Services.AppServices.Services; 

public class RedisCache: AppServiceBase, IRedisCacheService {
    private bool IsEnabled { get; }
    private int SlidingExpiration { get; }
    private int AbsoluteExpiration { get; }

    private readonly IDistributedCache _redisCache;

    public RedisCache() { }

    public RedisCache(
        IConfiguration configuration,
        IEcosystem ecosystem,
        ILoggerService logger,
        IDistributedCache redisCache
    ): base(logger) {
        _redisCache = redisCache;

        var environment = ecosystem.GetEnvironment();
        var (isEnabled, slidingExpiration, absoluteExpiration) = (
            bool.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.IsEnabled)}")!),
            int.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.SlidingExpiration)}")!),
            int.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.AbsoluteExpiration)}")!)
        );

        IsEnabled = isEnabled;
        SlidingExpiration = slidingExpiration;
        AbsoluteExpiration = absoluteExpiration;
    }

    public virtual async Task InsertCacheEntry(ICacheEntry entry) {
        _logger.Log(new LoggerBinding<RedisCache> { Location = nameof(InsertCacheEntry) });
        if (!IsEnabled) return;

        await _redisCache.SetAsync(
            ((RedisCacheEntry)entry).Key,
            ((RedisCacheEntry)entry).Value.EncodeDataUtf8(),
            new DistributedCacheEntryOptions {
                SlidingExpiration = TimeSpan.FromSeconds(SlidingExpiration),
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(AbsoluteExpiration)
            }
        );
    }

    public virtual async Task<T?> GetCacheEntry<T>(string key) {
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

    public virtual async Task RemoveCacheEntry(string key) {
        _logger.Log(new LoggerBinding<RedisCache> { Location = nameof(RemoveCacheEntry) });
        if (!IsEnabled) return;
        await _redisCache.RemoveAsync(key);
    }
}