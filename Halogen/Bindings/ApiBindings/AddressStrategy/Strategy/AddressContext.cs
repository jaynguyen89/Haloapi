namespace Halogen.Bindings.ApiBindings.AddressStrategy.Strategy;

// Strategy Pattern
public sealed class AddressContext {

    private readonly IAddressStrategy _address;

    public AddressContext(IAddressStrategy address) {
        _address = address;
    }

    public Dictionary<string, List<string>> DoVerification() => _address.VerifyAddressData();
    public string DoToString() => _address.ToString();
}