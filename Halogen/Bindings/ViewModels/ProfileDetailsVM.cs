using Halogen.DbModels;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using Newtonsoft.Json;

namespace Halogen.Bindings.ViewModels;

public sealed class ProfileDetailsVM {

    public string? GivenName { get; set; }

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    public string? FullName { get; set; }
    
    public string? NickName { get; set; }
    
    public Enums.Gender Gender { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    public Enums.Ethnicity Ethnicity { get; set; }

    public WorkAndInterestVM WorkAndInterest { get; set; } = null!;

    public static implicit operator ProfileDetailsVM(Profile profile) {
        var fullName = profile.FullName
                       ?? $"{profile.GivenName ?? string.Empty} {profile.MiddleName ?? string.Empty} {profile.LastName ?? string.Empty}";
        if (!fullName.IsString()) fullName = null;
        
        
        return new ProfileDetailsVM {
            GivenName = profile.GivenName,
            MiddleName = profile.MiddleName,
            LastName = profile.LastName,
            FullName = fullName,
            NickName = profile.NickName,
            Gender = (Enums.Gender)profile.Gender,
            DateOfBirth = profile.DateOfBirth,
            Ethnicity = (Enums.Ethnicity)profile.Ethnicity,
            WorkAndInterest = profile,
        };
    }
}

public sealed class WorkAndInterestVM {
    
    public string? Company  { get; set; }
    
    public string? JobTitle  { get; set; }
    
    public InterestVM[]? Interests  { get; set; }
    
    public ProfileLinkVM[]? ProfileLinks  { get; set; }

    public static implicit operator WorkAndInterestVM(Profile profile) {
        var profileLinks = profile.Websites is null
            ? null
            : ConvertWebsites(profile.Websites);
        
        return new WorkAndInterestVM {
            Company = profile.Company,
            JobTitle = profile.JobTitle,
            ProfileLinks = profileLinks,
        };

        ProfileLinkVM[] ConvertWebsites(string websitesJson) => JsonConvert.DeserializeObject<ProfileLinkVM[]>(websitesJson) ?? [];
    }
}

public sealed class ProfileLinkVM {
    
    public Enums.SocialMedia LinkType { get; set; }

    public string LinkHref { get; set; } = null!;
}
