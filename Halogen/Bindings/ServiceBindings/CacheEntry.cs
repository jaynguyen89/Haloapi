using Microsoft.Extensions.Caching.Memory;

namespace Halogen.Bindings.ServiceBindings;

public interface ICacheEntry;

public class RedisCacheEntry: ICacheEntry {

    internal string Key { get; set; } = null!;

    internal object Value { get; set; } = null!;
}

public sealed class MemoryCacheEntry: RedisCacheEntry {
    
    internal DateTimeOffset? AbsoluteExpiration { get; set; } = null;
    
    internal CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;
}