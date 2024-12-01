using Halogen.Bindings;
using Halogen.Bindings.ServiceBindings;
using Halogen.Services.AppServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Caching.Memory;
using ICacheEntry = Halogen.Bindings.ServiceBindings.ICacheEntry;

namespace Halogen.Services.AppServices.Services;

public class MemoryCache: AppServiceBase, IMemoryCacheService {

    private bool IsEnabled { get; } = true;
    
    private readonly Microsoft.Extensions.Caching.Memory.MemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _entryOptions;

    public MemoryCache() { }

    public MemoryCache(
        IConfiguration configuration,
        IEcosystem ecosystem,
        ILoggerService logger
    ): base(logger) {
        var environment = ecosystem.GetEnvironment();
        IsEnabled = bool.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.MemoryCacheSettings.IsEnabled)}")!);
        
        _memoryCache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new MemoryCacheOptions {
            SizeLimit = int.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.MemoryCacheSettings.Size)}")!),
            CompactionPercentage = double.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.MemoryCacheSettings.Compaction)}")!),
            ExpirationScanFrequency = TimeSpan.FromSeconds(int.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.MemoryCacheSettings.ScanFrequency)}")!)),
        });

        _entryOptions = new MemoryCacheEntryOptions {
            SlidingExpiration = TimeSpan.FromSeconds(int.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.MemoryCacheSettings.SlidingExpiration)}")!)),
        };
    }
    
    public virtual Task InsertCacheEntry(ICacheEntry entry) {
        _logger.Log(new LoggerBinding<MemoryCache> { Location = nameof(InsertCacheEntry) });
        if (!IsEnabled) return Task.CompletedTask;
        
        _entryOptions.Priority = ((MemoryCacheEntry)entry).Priority;
        _entryOptions.AbsoluteExpiration = ((MemoryCacheEntry)entry).AbsoluteExpiration;
        
        _memoryCache.Set(((MemoryCacheEntry)entry).Key, ((MemoryCacheEntry)entry).Value, _entryOptions);
        return Task.CompletedTask;
    }

    public virtual Task<T?> GetCacheEntry<T>(string key) {
        _logger.Log(new LoggerBinding<MemoryCache> { Location = nameof(GetCacheEntry) });
        if (!IsEnabled) return Task.FromResult<T?>(default);

        try {
            var entry = _memoryCache.Get<T>(key);
            return Task.FromResult(entry);
        }
        catch (Exception) {
            return Task.FromResult<T?>(default);
        }
    }

    public virtual Task RemoveCacheEntry(string key) {
        _logger.Log(new LoggerBinding<MemoryCache> { Location = nameof(RemoveCacheEntry) });
        _memoryCache.Remove(key);
        return Task.CompletedTask;
    }
}