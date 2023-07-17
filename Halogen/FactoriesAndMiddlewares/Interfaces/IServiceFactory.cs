using Halogen.Services;
using HelperLibrary.Shared;

namespace Halogen.FactoriesAndMiddlewares.Interfaces; 

public interface IServiceFactory {
    
    IServiceBase GetService<T>(Enums.ServiceType serviceType);
}