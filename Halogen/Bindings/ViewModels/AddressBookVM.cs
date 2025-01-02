using Halogen.Bindings.ApiBindings.AddressStrategy;
using Halogen.DbModels;
using HelperLibrary.Shared;
using Address = Halogen.DbModels.Address;

namespace Halogen.Bindings.ViewModels;

public sealed class AddressBookVM {

    public string ProfileId { get; set; } = null!;
    
    public AddressVM[] Addresses { get; set; } = null!;
}

public sealed class AddressVM {

    public string Id { get; set; } = null!;
    
    public bool IsForPostage  { get; set; }
    
    public bool IsForDelivery  { get; set; }
    
    public bool IsForReturn  { get; set; }
    
    public IAddress Address { get; set; } = null!;

    public static AddressVM CreateAddressVm(ProfileAddress dbProfileAddress, Address dbAddress) {
        IAddress address = dbAddress.Variant == (byte)Enums.AddressVariant.Eastern
            ? (WesternAddress)dbAddress
            : (EasternAddress)dbAddress;

        return new AddressVM {
            Id = dbAddress.Id,
            IsForPostage = dbProfileAddress.IsForPostage,
            IsForDelivery = dbProfileAddress.IsForDelivery,
            IsForReturn = dbProfileAddress.IsForReturn,
            Address = address,
        };
    }
}
