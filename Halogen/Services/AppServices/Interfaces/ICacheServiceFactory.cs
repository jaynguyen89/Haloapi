namespace Halogen.Services.AppServices.Interfaces;

public interface ICacheServiceFactory {
    ICacheService GetActiveCacheService();
}