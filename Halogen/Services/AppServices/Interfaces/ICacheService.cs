using Halogen.Bindings.ServiceBindings;

namespace Halogen.Services.AppServices.Interfaces;

public interface ICacheService {
    Task InsertCacheEntry(ICacheEntry entry);

    Task<T?> GetCacheEntry<T>(string key);

    Task RemoveCacheEntry(string key);
}

public interface IMemoryCacheService: ICacheService;

public interface IRedisCacheService: ICacheService;