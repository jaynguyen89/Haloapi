﻿using Halogen.Bindings.ServiceBindings;

namespace Halogen.Services.AppServices.Interfaces;

public interface ICacheService {

    bool IsEnabled { get; set; }
    
    int SlidingExpiration { get; set; }
    
    int AbsoluteExpiration { get; set; }

    Task InsertCacheEntry(CacheEntry entry);

    Task<T?> GetCacheEntry<T>(string key);

    Task RemoveCacheEntry(string key);
}