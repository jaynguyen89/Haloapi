using HelperLibrary.Shared;

namespace Halogen.FactoriesAndMiddlewares.Interfaces; 

public interface IHaloServiceFactory {
    
    T? GetService<T>(Enums.ServiceType serviceType);
}