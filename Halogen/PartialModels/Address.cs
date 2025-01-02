using Halogen.Bindings.ApiBindings;
using HelperLibrary.Shared.Helpers;

// ReSharper disable once CheckNamespace
namespace Halogen.DbModels;

public partial class Address {
    public static Address CreateNewAddress(AddressData addressData) => new() {
        Id = StringHelpers.NewGuid(),
        BuildingName = addressData.Address.BuildingName,
        PoBoxNumber = addressData.Address.PoBoxNumber,
        StreetAddress = addressData.Address.StreetAddress,
        Group = addressData.Address.Group,
        Lane = addressData.Address.Lane,
        Quarter = addressData.Address.Quarter,
        Hamlet = addressData.Address.Hamlet,
        Commute = addressData.Address.Commute,
        Ward = addressData.Address.Ward,
        District = addressData.Address.District,
        Town = addressData.Address.Town,
        Suburb = addressData.Address.Suburb,
        Postcode = addressData.Address.Postcode,
        City = addressData.Address.City,
        DivisionId = addressData.Address.DivisionId,
        CountryId = addressData.Address.CountryId,
        Variant = addressData.Address.Variant,
    };

    public void UpdateAddress(AddressData addressData) {
        BuildingName = addressData.Address.BuildingName;
        PoBoxNumber = addressData.Address.PoBoxNumber;
        StreetAddress = addressData.Address.StreetAddress;
        Group = addressData.Address.Group;
        Lane = addressData.Address.Lane;
        Quarter = addressData.Address.Quarter;
        Hamlet = addressData.Address.Hamlet;
        Commute = addressData.Address.Commute;
        Ward = addressData.Address.Ward;
        District = addressData.Address.District;
        Town = addressData.Address.Town;
        Suburb = addressData.Address.Suburb;
        Postcode = addressData.Address.Postcode;
        City = addressData.Address.City;
        DivisionId = addressData.Address.DivisionId;
        CountryId = addressData.Address.CountryId;
        Variant = addressData.Address.Variant;
    }
}