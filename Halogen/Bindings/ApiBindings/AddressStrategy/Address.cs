using System.Text.RegularExpressions;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;

namespace Halogen.Bindings.ApiBindings.AddressStrategy;

public interface IAddress;

public class Address: IAddress {
    
    public string Id { get; set; } = null!;
    
    public Enums.AddressVariant Variant { get; set; }
    
    public string? BuildingName { get; set; }
    
    public string? PoBoxNumber { get; set; }

    public string StreetAddress { get; set; } = null!;
    
    public Division Division { get; set; } = null!;
    
    public Country Country { get; set; } = null!;

    protected Dictionary<string, List<string>> VerifyCommonAddressParts() {
        var buildingErrors = new List<string>();
        BuildingName = Regex.Replace(BuildingName?.Trim() ?? string.Empty, Constants.MultiSpace, Constants.MonoSpace);
        if (BuildingName.IsString()) buildingErrors = BuildingName.VerifyInformalName(nameof(BuildingName));
        else BuildingName = null;
        
        var poBoxErrors = new List<string>();
        PoBoxNumber = Regex.Replace(PoBoxNumber?.Trim() ?? string.Empty, Constants.MultiSpace, Constants.MonoSpace);
        if (PoBoxNumber.IsString()) poBoxErrors = PoBoxNumber.VerifyLength("PO Box", 2);
        else PoBoxNumber = null;
        
        StreetAddress = Regex.Replace(StreetAddress.Trim(), Constants.MultiSpace, Constants.MonoSpace);
        var streetErrors = StreetAddress.VerifyLength(nameof(StreetAddress).Lucidify(), 5, 100);
        
        return ListHelpers.MergeDataValidationErrors(
            new KeyValuePair<string, List<string>>(nameof(BuildingName), buildingErrors),
            new KeyValuePair<string, List<string>>(nameof(PoBoxNumber), poBoxErrors),
            new KeyValuePair<string, List<string>>(nameof(StreetAddress), streetErrors)
        );
    }
}

public sealed class UnifiedAddress {
    
    public byte Variant { get; set; }
    
    public string? BuildingName { get; set; }
    
    public string? PoBoxNumber { get; set; }

    public string StreetAddress { get; set; } = null!;
    
    public string? Group { get; set; }
    
    public string? Lane { get; set; }
    
    public string? Quarter { get; set; }
    
    public string? Hamlet { get; set; }
    
    public string? Commute { get; set; }
    
    public string? Ward { get; set; }
    
    public string? District { get; set; }
    
    public string? Town { get; set; }
    
    public string? City { get; set; }
    
    public string? Suburb { get; set; }
    
    public string? Postcode { get; set; }
    
    public string DivisionId { get; set; } = null!;
    
    public string CountryId { get; set; } = null!;
}

public sealed class Country {
    
    public string Id { get; set; } = null!;
    
    public string Name { get; set; } = null!;
    
    public Enums.LocalityRegion Region { get; set; }
}

public sealed class Division {
    
    public string Id { get; set; } = null!;
    
    public string CountryId { get; set; } = null!;
    
    public Enums.DivisionType Type { get; set; }
    
    public string Name { get; set; } = null!;
    
    public string? Abbreviation { get; set; }
}
