using Halogen.DbModels;
using HelperLibrary.Shared.Helpers;

namespace Halogen.Bindings.ViewModels; 

public sealed class PublicDataVM {

    public string Environment { get; set; } = null!;
    
    public bool SecretCodeEnabled { get; set; }
    
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

    public EnumProp[] Visibilities { get; set; } = null!;

    public CountryVM[] Countries { get; set; } = null!;

    public string[] SupportedSocialAccounts { get; set; } = null!;
    
    public EnumProp[] SocialMedias { get; set; } = null!;
    
    public EnumProp[] LocalityRegions { get; set; } = null!;
    
    public EnumProp[] Ethnicities { get; set; } = null!;
}