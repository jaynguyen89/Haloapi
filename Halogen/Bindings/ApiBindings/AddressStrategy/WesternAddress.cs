using System.Text.RegularExpressions;
using Halogen.Bindings.ApiBindings.AddressStrategy.Strategy;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;

namespace Halogen.Bindings.ApiBindings.AddressStrategy;

public sealed class WesternAddress: Address, IAddressStrategy {

    public string Suburb { get; set; } = null!;

    public string Postcode { get; set; } = null!;
    
    public Dictionary<string, List<string>> VerifyAddressData() {
        Suburb = Regex.Replace(Suburb.Trim(), Constants.MultiSpace, Constants.MonoSpace);
        var suburbErrors = Suburb.VerifyLength(nameof(Suburb), 1, 100);
        
        Postcode = Regex.Replace(Postcode.Trim(), Constants.MultiSpace, Constants.MonoSpace);
        var postcodeErrors = Postcode.VerifyNumeralString(nameof(Postcode), 4, 10);

        var allErrors = VerifyCommonAddressParts();
        allErrors.MergeDataValidationErrors(
            new KeyValuePair<string, List<string>>(nameof(Suburb), suburbErrors),
            new KeyValuePair<string, List<string>>(nameof(Postcode), postcodeErrors)
        );
        
        return allErrors;
    }

    public override string ToString() {
        var poBox = PoBoxNumber.IsString() ? $"{PoBoxNumber}, " : string.Empty;
        var building = BuildingName.IsString() ? $"{BuildingName}, " : string.Empty;
        return $"{poBox}{building}{StreetAddress}, {Suburb}, {Division.Name} {Postcode}, {Country.Name}";
    }

    public static implicit operator WesternAddress(DbModels.Address address) => new() {
        Id = address.Id,
        Variant = (Enums.AddressVariant)address.Variant,
        BuildingName = address.BuildingName,
        PoBoxNumber = address.PoBoxNumber,
        StreetAddress = address.StreetAddress,
        Suburb = address.Suburb ?? string.Empty,
        Postcode = address.Postcode ?? string.Empty,
        Division = new Division {
            Id = address.DivisionId,
            CountryId = address.CountryId,
        },
    };
}
