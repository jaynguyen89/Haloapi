﻿using Halogen.DbModels;
using HelperLibrary.Shared.Helpers;

namespace Halogen.Bindings.ViewModels; 

public sealed class PublicData {

    public string Environment { get; set; } = null!;
    
    public bool EnableSecretCode { get; set; }
    
    public int SecretCodeLength { get; set; }

    public EnumProp[] DateFormats { get; set; } = null!;

    public EnumProp[] TimeFormats { get; set; } = null!;

    public EnumProp[] NumberFormats { get; set; } = null!;

    public EnumProp[] Genders { get; set; } = null!;

    public EnumProp[] Languages { get; set; } = null!;

    public EnumProp[] Themes { get; set; } = null!;

    public EnumProp[] NameFormats { get; set; } = null!;

    public EnumProp[] BirthFormats { get; set; } = null!;

    public EnumProp[] PhoneNumberFormats { get; set; } = null!;

    public EnumProp[] UnitSystems { get; set; } = null!;

    public EnumProp[] CareerFormats { get; set; } = null!;

    public EnumProp[] VisibilityFormats { get; set; } = null!;

    public CountryData[] Countries { get; set; } = null!;
    
    public sealed class CountryData {

        public string Name { get; set; } = null!;
        
        public string IsoCode2Char { get; set; } = null!;
        
        public string IsoCode3Char { get; set; } = null!;
        
        public string TelephoneCode { get; set; } = null!;

        public static implicit operator CountryData(Locality locality) => new() {
            Name = locality.Name,
            IsoCode2Char = locality.IsoCode2Char,
            IsoCode3Char = locality.IsoCode3Char,
            TelephoneCode = locality.TelephoneCode,
        };
    }
}