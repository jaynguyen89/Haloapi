namespace Halogen.Bindings.ApiBindings.AddressStrategy.Strategy;

// Strategy Pattern
public interface IAddressStrategy {
    
    Dictionary<string, List<string>> VerifyAddressData();

    string ToString();
}
