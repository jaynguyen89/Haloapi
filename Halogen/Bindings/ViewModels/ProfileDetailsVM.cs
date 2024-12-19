using HelperLibrary.Shared;

namespace Halogen.Bindings.ViewModels;

public sealed class ProfileDetailsVM {

    public string GivenName { get; set; } = null!;

    public string MiddleName { get; set; } = null!;

    public string FamilyName { get; set; } = null!;

    public string FullName { get; set; } = null!;
    
    public string NickName { get; set; } = null!;
    
    public Enums.Gender Gender { get; set; }
    
    public DateTime? Birthday { get; set; }
    
    public string Ethnicity { get; set; } = null!;
}

public sealed class WorkAndInterest {
    
    public string? Company  { get; set; }
    
    public string? JobTitle  { get; set; }
    
    public InterestVM[]? Interests  { get; set; }
    
    public ProfileLinkVM[]? ProfileLinks  { get; set; }
}

public sealed class ProfileLinkVM {
    
    public Enums.SocialMedia LinkType { get; set; }

    public string LinkHref { get; set; } = null!;
}
