namespace Halogen.Services.AppServices.Interfaces;

public interface ICacheServiceFactory {
    
    /// <summary>
    /// To get the active cache service, which is the concrete type of ICacheService.
    /// </summary>
    /// <returns>ICacheService</returns>
    ICacheService GetActiveCacheService();
}