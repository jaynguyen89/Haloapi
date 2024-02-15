using HelperLibrary.Shared;

namespace Halogen.Auxiliaries.Interfaces; 

public interface IHaloServiceFactory {
    
    T? GetService<T>(Enums.ServiceType serviceType);
}