using Halogen.Bindings.ServiceBindings;

namespace Halogen.Auxiliaries.Interfaces;

public interface IHaloConfigProvider {

    HalogenConfigs GetHalogenConfigs();
}