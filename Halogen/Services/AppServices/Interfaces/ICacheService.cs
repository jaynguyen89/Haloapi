using Halogen.Bindings.ServiceBindings;

namespace Halogen.Services.AppServices.Interfaces;

public interface ICacheService {
    
    /// <summary>
    /// To insert data into the active cache service.
    /// </summary>
    /// <param name="entry">ICacheEntry</param>
    /// <returns>void</returns>
    Task InsertCacheEntry(ICacheEntry entry);

    /// <summary>
    /// To get data in T type from the active cache service.
    /// </summary>
    /// <param name="key">string</param>
    /// <typeparam name="T">type</typeparam>
    /// <returns>T?</returns>
    Task<T?> GetCacheEntry<T>(string key);

    /// <summary>
    /// To remove data from the active cache service using its key.
    /// </summary>
    /// <param name="key">string</param>
    /// <returns>void</returns>
    Task RemoveCacheEntry(string key);
}

public interface IMemoryCacheService: ICacheService;

public interface IRedisCacheService: ICacheService;