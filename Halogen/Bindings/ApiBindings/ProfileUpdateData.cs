using System.Text.RegularExpressions;
using Halogen.Auxiliaries.Interfaces;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;

namespace Halogen.Bindings.ApiBindings;

public sealed class ProfileUpdateData: ValueData {

    public string FieldName { get; set; } = null!;

    public async Task<Dictionary<string, List<string>?>?> VerifyProfileUpdateData(ProfileUpdateDataHandler dataHandler) {
        var errors = new List<string>();
        switch (FieldName) {
            case nameof(Profile.GivenName):
            case nameof(Profile.MiddleName):
            case nameof(Profile.LastName):
            case nameof(Profile.FullName):
            case nameof(Profile.NickName):
            case nameof(Profile.Company):
            case nameof(Profile.JobTitle):
                if (!StrValue.IsString()) StrValue = null;
                else {
                    StrValue = Regex.Replace(StrValue!.Trim(), Constants.MultiSpace, Constants.MonoSpace);
                    errors = StrValue.VerifyFormalName(FieldName);
                }

                break;
            case nameof(Profile.DateOfBirth):
                if (!StrValue.IsString()) StrValue = null;
                else {
                    var isDateTime = DateTime.TryParse(StrValue, out _);
                    if (!isDateTime) errors.Add($"{FieldName.Lucidify()} seems to be invalid.");
                }

                break;
            case nameof(Profile.Gender):
                if (!IntValue.HasValue) IntValue = (byte)Enums.Gender.NotSpecified;
                else if (IntValue < 0 || IntValue > EnumHelpers.Length<Enums.Gender>())
                    errors.Add($"{FieldName.Lucidify()} is not recognized.");

                break;
            case nameof(Profile.Ethnicity):
                if (!IntValue.HasValue) IntValue = (byte)Enums.Ethnicity.NotSpecified;
                else if (IntValue < 0 || IntValue > EnumHelpers.Length<Enums.Ethnicity>())
                    errors.Add($"{FieldName.Lucidify()} is not recognized.");
                
                break;
            case nameof(Profile.Websites):
                foreach (var (websiteType, websiteLink) in IntValueMaps!) {
                    if (websiteType < 0 || websiteType > EnumHelpers.Length<Enums.SocialMedia>())
                        errors.Add($"{nameof(Enums.SocialMedia).Lucidify()} Type is not recognized.)");
                    
                    if (!websiteLink.IsValidUrl())
                        errors.Add($"{nameof(Enums.SocialMedia).Lucidify()} URL is invalid.");
                }
                break;
            case nameof(Profile.Interests):
                if (StrValues is not null) errors = await dataHandler.VerifyInterests(StrValues!);
                break;
            default:
                errors.Add($"Field `{FieldName}` is not recognized.");
                break;
        }

        return errors is null ? null : errors.Count == 0
            ? new Dictionary<string, List<string>?>()
            : new Dictionary<string, List<string>?> {{ FieldName, errors }};
    }
}

public sealed class ProfileUpdateDataHandler {

    private readonly IInterestService _interestService;

    public ProfileUpdateDataHandler(IHaloServiceFactory haloServiceFactory) {
        _interestService = haloServiceFactory.GetService<InterestService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<ProfileUpdateDataHandler>(nameof(InterestService));
    }

    public async Task<List<string>?> VerifyInterests(string[] interestIds) {
        var dbInterestIds = await _interestService.GetAllIds();
        if (dbInterestIds is null) return null;
        
        var interestIdsNotInDb = interestIds
            .Where(interestId => !dbInterestIds.Contains(interestId))
            .ToList();
        
        return interestIdsNotInDb.Count == 0 ? [] : interestIdsNotInDb;
    }
}
