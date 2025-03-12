using Halogen.DbModels;
using HelperLibrary.Shared;

namespace Halogen.Bindings.ViewModels;

public sealed class CountryVM {
        
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;
        
    public string IsoCode2Char { get; set; } = null!;
        
    public string IsoCode3Char { get; set; } = null!;
        
    public string? TelephoneCode { get; set; }
    
    public Enums.LocalityRegion Region { get; set; }
    
    public string? PrimaryCurrencyId { get; set; }
    
    public string? SecondaryCurrencyId { get; set; }

    public static implicit operator CountryVM(Locality locality) => new() {
        Id = locality.Id,
        Name = locality.Name,
        IsoCode2Char = locality.IsoCode2Char,
        IsoCode3Char = locality.IsoCode3Char,
        TelephoneCode = locality.TelephoneCode,
    };
}

public sealed class DivisionVM {
    
    public string Id { get; set; } = null!;
    
    public string? CountryId { get; set; } = null!;
    
    public string Name { get; set; } = null!;
    
    public Enums.DivisionType DivisionType { get; set; }
    
    public string? Abbreviation { get; set; }

    public static implicit operator DivisionVM(LocalityDivision division) => new() {
        Id = division.Id,
        CountryId = division.LocalityId,
        Name = division.Name,
        DivisionType = (Enums.DivisionType)division.DivisionType,
        Abbreviation = division.Abbreviation,
    };
}

public sealed class LocalityVM {
    public CountryVM Country { get; set; } = null!;
    public DivisionVM[] Divisions { get; set; } = null!;
}