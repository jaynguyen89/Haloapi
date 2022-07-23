using Halogen.Bindings.ServiceBindings;
using Halogen.Parsers;
using Halogen.Services.AppServices.Interfaces;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.Extensions.Caching.Distributed;

namespace Halogen.Services.AppServices.Services; 

internal sealed class RedisCache: ICacheService {

    public bool IsEnabled { get; set; }
    public int SlidingExpiration { get; set; }
    public int AbsoluteExpiration { get; set; }

    private readonly IDistributedCache _redisCache;

    internal RedisCache(IDistributedCache redisCache, IConfiguration configuration) {
        _redisCache = redisCache;
        
        var environment = configuration.GetValue<string>($"{nameof(Halogen)}Environment");
        var (isEnabled, slidingExpiration, absoluteExpiration) = environment switch {
            Constants.Development => (
                bool.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.CacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Development.CacheSettings.IsEnabled)}")),
                int.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.CacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Development.CacheSettings.SlidingExpiration)}")),
                int.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.CacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Development.CacheSettings.AbsoluteExpiration)}"))
            ),
            Constants.Staging => (
                bool.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.CacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Staging.CacheSettings.IsEnabled)}")),
                int.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.CacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Staging.CacheSettings.SlidingExpiration)}")),
                int.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.CacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Staging.CacheSettings.AbsoluteExpiration)}"))
            ),
            _ => (
                bool.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.CacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Production.CacheSettings.IsEnabled)}")),
                int.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.CacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Production.CacheSettings.SlidingExpiration)}")),
                int.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.CacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Production.CacheSettings.AbsoluteExpiration)}"))
            )
        };

        IsEnabled = isEnabled;
        SlidingExpiration = slidingExpiration;
        AbsoluteExpiration = absoluteExpiration;
    }

    public async Task InsertCacheEntry(CacheEntry entry) {
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
        if (!IsEnabled) return;
        await _redisCache.RemoveAsync(key);
    }
}