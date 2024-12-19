using Halogen.Bindings.ApiBindings.AddressStrategy;

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
}
