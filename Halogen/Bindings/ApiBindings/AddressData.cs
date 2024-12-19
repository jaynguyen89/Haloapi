using Halogen.Bindings.ApiBindings.AddressStrategy;
using Halogen.Bindings.ApiBindings.AddressStrategy.Strategy;
using HelperLibrary.Shared;

namespace Halogen.Bindings.ApiBindings;

public sealed class AddressData {
    
    public bool IsForPostage  { get; set; }
    
    public bool IsForDelivery  { get; set; }
    
    public bool IsForReturn  { get; set; }

    public UnifiedAddress Address { get; set; } = null!;

    public Dictionary<string, List<string>> VerifyAddressData() {
        IAddressStrategy categorizedAddress = Address.Variant == (byte)Enums.AddressVariant.Eastern
            ? new EasternAddress {
                Variant = (Enums.AddressVariant)Address.Variant,
                BuildingName = Address.BuildingName,
                PoBoxNumber = Address.PoBoxNumber,
                StreetAddress = Address.StreetAddress,
                Group = Address.Group,
                Lane = Address.Lane,
                Quarter = Address.Quarter,
                Hamlet = Address.Hamlet,
                Commute = Address.Commute,
                Ward = Address.Ward,
                District = Address.District,
                Town = Address.Town,
                City = Address.City,
            }
            : new WesternAddress {
                Variant = (Enums.AddressVariant)Address.Variant,
                BuildingName = Address.BuildingName,
                PoBoxNumber = Address.PoBoxNumber,
                StreetAddress = Address.StreetAddress,
                Suburb = Address.Suburb!,
                Postcode = Address.Postcode!,
            };

        var addressContext = new AddressContext(categorizedAddress);
        return addressContext.DoVerification();
    }
}
