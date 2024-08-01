using Halogen.Bindings.ServiceBindings;
using Halogen.Services.AppServices.Interfaces;
using Halogen.Services.AppServices.Services;
using Moq;

namespace HaloUnitTest.Mocks.HaloApi.Services;

// Singleton
internal sealed class CacheServiceMock: MockBase {

    private static readonly Lazy<CacheServiceMock> CacheSvMock = new(() => new CacheServiceMock());

    private readonly Mock<RedisCache> _cacheSvMock;

    private CacheServiceMock() {
        _cacheSvMock = Simulate<RedisCache>();
    }

    internal static Mock<RedisCache> Instance() => CacheSvMock.Value._cacheSvMock;

    internal static RedisCache Instance<T, TK>(string propertyName, T value, TK returnVal) {
        var cacheSvMock = Instance();
        switch (propertyName) {
            case nameof(ICacheService.InsertCacheEntry):
                var cacheEntry = (CacheEntry)(object)value!;
                cacheSvMock.Setup(m => m.InsertCacheEntry(cacheEntry)).Returns(Task.CompletedTask);
                break;
            case nameof(ICacheService.GetCacheEntry):
                cacheSvMock.Setup(m => m.GetCacheEntry<TK>((string)(object)value!)).ReturnsAsync(returnVal);
                break;
            case nameof(ICacheService.RemoveCacheEntry):
                cacheSvMock.Setup(m => m.RemoveCacheEntry((string)(object)value!)).Returns(Task.CompletedTask);
                break;
        }

        return cacheSvMock.Object;
    }
}
