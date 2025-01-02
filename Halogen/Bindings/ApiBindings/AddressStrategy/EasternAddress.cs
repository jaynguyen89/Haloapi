using System.Text.RegularExpressions;
using Halogen.Bindings.ApiBindings.AddressStrategy.Strategy;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;

namespace Halogen.Bindings.ApiBindings.AddressStrategy;

public sealed class EasternAddress: Address, IAddressStrategy {
    
    public string? Lane { get; set; }
    
    public string? Group { get; set; }
    
    public string? Quarter { get; set; }
    
    public string? Hamlet { get; set; }
    
    public string? Commute { get; set; }
    
    public string? Ward { get; set; }
    
    public string? District { get; set; }
    
    public string? Town { get; set; }
    
    public string? City { get; set; }
    
    public Dictionary<string, List<string>> VerifyAddressData() {
        var laneErrors = new List<string>();
        Lane = Regex.Replace(Lane?.Trim() ?? string.Empty, Constants.MultiSpace, Constants.MonoSpace);
        if (Lane.IsString()) laneErrors = Lane.VerifyLength(nameof(Lane));
        else Lane = null;
        
        var groupErrors = new List<string>();
        Group = Regex.Replace(Group?.Trim() ?? string.Empty, Constants.MultiSpace, Constants.MonoSpace);
        if (Group.IsString()) groupErrors = Group.VerifyLength(nameof(Group));
        else Group = null;
        
        var quarterErrors = new List<string>();
        Quarter = Regex.Replace(Quarter?.Trim() ?? string.Empty, Constants.MultiSpace, Constants.MonoSpace);
        if (Quarter.IsString()) quarterErrors = Quarter.VerifyLength(nameof(Quarter));
        else Quarter = null;
        
        var hamletErrors = new List<string>();
        Hamlet = Regex.Replace(Hamlet?.Trim() ?? string.Empty, Constants.MultiSpace, Constants.MonoSpace);
        if (Hamlet.IsString()) hamletErrors = Hamlet.VerifyLength(nameof(Hamlet));
        else Hamlet = null;
        
        var commuteErrors = new List<string>();
        Commute = Regex.Replace(Commute?.Trim() ?? string.Empty, Constants.MultiSpace, Constants.MonoSpace);
        if (Commute.IsString()) commuteErrors = Commute.VerifyLength(nameof(Commute));
        else Commute = null;
        
        var wardErrors = new List<string>();
        Ward = Regex.Replace(Ward?.Trim() ?? string.Empty, Constants.MultiSpace, Constants.MonoSpace);
        if (Ward.IsString()) wardErrors = Ward.VerifyLength(nameof(Ward));
        else Ward = null;
        
        var districtErrors = new List<string>();
        District = Regex.Replace(District?.Trim() ?? string.Empty, Constants.MultiSpace, Constants.MonoSpace);
        if (District.IsString()) districtErrors = District.VerifyLength(nameof(District));
        else District = null;
        
        var townErrors = new List<string>();
        Town = Regex.Replace(Town?.Trim() ?? string.Empty, Constants.MultiSpace, Constants.MonoSpace);
        if (Town.IsString()) townErrors = Town.VerifyLength(nameof(Town));
        else Town = null;
        
        var cityErrors = new List<string>();
        City = Regex.Replace(City?.Trim() ?? string.Empty, Constants.MultiSpace, Constants.MonoSpace);
        if (City.IsString()) cityErrors = City.VerifyLength(nameof(City));
        else City = null;

        var allErrors = VerifyCommonAddressParts();
        allErrors.MergeDataValidationErrors(
            new KeyValuePair<string, List<string>>(nameof(Group), groupErrors),
            new KeyValuePair<string, List<string>>(nameof(Lane), laneErrors),
            new KeyValuePair<string, List<string>>(nameof(Quarter), quarterErrors),
            new KeyValuePair<string, List<string>>(nameof(Hamlet), hamletErrors),
            new KeyValuePair<string, List<string>>(nameof(Commute), commuteErrors),
            new KeyValuePair<string, List<string>>(nameof(Ward), wardErrors),
            new KeyValuePair<string, List<string>>(nameof(District), districtErrors),
            new KeyValuePair<string, List<string>>(nameof(Town), townErrors),
            new KeyValuePair<string, List<string>>(nameof(City), cityErrors)
        );

        return allErrors;
    }

    public override string ToString() {
        var poBox = PoBoxNumber.IsString() ? $"{PoBoxNumber}, " : string.Empty;
        var building = BuildingName.IsString() ? $"{BuildingName}, " : string.Empty;
        var lane = Lane.IsString() ? $"{Lane}, " : string.Empty;
        var group = Group.IsString() ? $"{Group}, " : string.Empty;
        var quarter = Quarter.IsString() ? $"{Quarter}, " : string.Empty;
        var hamlet = Hamlet.IsString() ? $"{Hamlet}, " : string.Empty;
        var commute = Commute.IsString() ? $"{Commute}, " : string.Empty;
        var ward = Ward.IsString() ? $"{Ward}, " : string.Empty;
        var district = District.IsString() ? $"{District}, " : string.Empty;
        var town = Town.IsString() ? $"{Town}, " : string.Empty;
        var city = City.IsString() ? $"{City}, " : string.Empty;

        return $"{poBox}{building}{lane}{StreetAddress}, {group}{quarter}{hamlet}{commute}{ward}{district}{town}{city}{Division.Name}, {Country.Name}";
    }

    public static implicit operator EasternAddress(DbModels.Address address) => new() {
        Id = address.Id,
        Variant = (Enums.AddressVariant)address.Variant,
        BuildingName = address.BuildingName,
        PoBoxNumber = address.PoBoxNumber,
        StreetAddress = address.StreetAddress,
        Lane = address.Lane,
        Group = address.Group,
        Quarter = address.Quarter,
        Hamlet = address.Hamlet,
        Commute = address.Commute,
        Ward = address.Ward,
        District = address.District,
        Town = address.Town,
        City = address.City,
        Division = new Division {
            Id = address.DivisionId,
            CountryId = address.CountryId,
        },
    };
}
