// ReSharper disable once CheckNamespace
namespace Halogen.DbModels;

public partial class Locality {
    public Locality(string id, string name, byte region, string isoCode2Char, string isoCode3Char, string phoneCode, string primaryCurrencyId, string? secondaryCurrencyId) {
        Id = id;
        Name = name;
        Region = region;
        IsoCode2Char = isoCode2Char;
        IsoCode3Char = isoCode3Char;
        TelephoneCode = phoneCode;
        PrimaryCurrencyId = primaryCurrencyId;
        SecondaryCurrencyId = secondaryCurrencyId;
        
        Addresses = new HashSet<Address>();
        LocalityDivisions = new HashSet<LocalityDivision>();
    }
}