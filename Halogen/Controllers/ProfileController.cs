using System.Net;
using AssistantLibrary;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces.IServiceFactory;
using AssistantLibrary.Services;
using Halogen.Attributes;
using Halogen.Bindings;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings.ApiBindings;
using Halogen.Bindings.ViewModels;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using MediaLibrary.Services;
using MediaLibrary.Services.DbServices;
using MediaLibrary.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Halogen.Controllers; 

[ApiController]
[Route("profile")]
[AutoValidateAntiforgeryToken]
[ServiceFilter(typeof(AuthenticatedAuthorize))]
[ServiceFilter(typeof(TwoFactorAuthorize))]
public sealed class ProfileController: AppController {
    
    private readonly IContextService _contextService;
    private readonly IProfileService _profileService;
    private readonly ISmsService _smsService;
    private readonly RegionalizedPhoneNumberHandler _phoneNumberHandler;
    private readonly ProfileUpdateDataHandler _profileUpdateDataHandler;
    private readonly IProfilePhotoService _profilePhotoService;

    public ProfileController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        IHaloServiceFactory haloServiceFactory,
        IHaloConfigProvider haloConfigProvider,
        IAssistantServiceFactory assistantServiceFactory,
        RegionalizedPhoneNumberHandler phoneNumberHandler,
        ProfileUpdateDataHandler profileUpdateDataHandler,
        IMediaServiceFactory mediaServiceFactory
    ) : base(ecosystem, logger, configuration, haloConfigProvider.GetHalogenConfigs()) {
        _contextService = haloServiceFactory.GetService<ContextService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<ProfileController>(nameof(ContextService));
        _profileService = haloServiceFactory.GetService<ProfileService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<ProfileController>(nameof(ProfileService));
        _smsService = assistantServiceFactory.GetService<SmsServiceFactory>()?.GetActiveSmsService() ?? throw new HaloArgumentNullException<ProfileController>(nameof(SmsServiceFactory));
        _phoneNumberHandler = phoneNumberHandler;
        _profileUpdateDataHandler = profileUpdateDataHandler;
        _profilePhotoService = mediaServiceFactory.GetService<ProfilePhotoService>() ?? throw new HaloArgumentNullException<ProfileController>(nameof(MediaServiceFactory));
    }

    [HttpGet("get-phone-number-credential")]
    public async Task<IActionResult> GetPhoneNumberCredential([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<ProfileController> { Location = nameof(GetPhoneNumberCredential) });
        
        var phoneNumberCredential = await _profileService.GetPhoneNumberCredential(accountId);
        return phoneNumberCredential is null
            ? new ErrorResponse()
            : new SuccessResponse(phoneNumberCredential);
    }

    [HttpPatch("request-phone-number-confirmation")]
    public async Task<IActionResult> RequestPhoneNumberConfirmation([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<ProfileController> { Location = nameof(RequestPhoneNumberConfirmation) });

        var profile = await _profileService.GetProfileByAccountId(accountId);
        if (profile is null) return new ErrorResponse();
        if (profile.PhoneNumberConfirmed) return new ErrorResponse(HttpStatusCode.Continue);
        if (profile.PhoneNumberToken.IsString())
            return profile.PhoneNumberTokenTimestamp!.Value.Compute(_haloConfigs.PhoneTokenValidityDuration, _haloConfigs.PhoneTokenValidityDurationUnit) < DateTime.UtcNow
                ? new ErrorResponse(HttpStatusCode.NotAcceptable)
                : new ErrorResponse(HttpStatusCode.Gone);

        return await UpdatePhoneNumberConfirmation(profile);
    }

    [HttpPatch("replace-phone-number")]
    public async Task<IActionResult> ReplacePhoneNumber([FromHeader] string accountId, [FromBody] RegionalizedPhoneNumber phoneNumber) {
        _logger.Log(new LoggerBinding<ProfileController> { Location = nameof(ReplacePhoneNumber) });
        
        var profile = await _profileService.GetProfileByAccountId(accountId);
        if (profile is null) return new ErrorResponse();
        
        var errors = await _phoneNumberHandler.VerifyPhoneNumberData(phoneNumber);
        if (errors.Length != 0) return new ErrorResponse(HttpStatusCode.BadRequest, errors);

        var isPhoneNumberAvailable = await _profileService.IsPhoneNumberAvailableForNewAccount(phoneNumber.Simplify());
        if (!isPhoneNumberAvailable.HasValue) return new ErrorResponse();
        if (!isPhoneNumberAvailable.Value) return new ErrorResponse(HttpStatusCode.Conflict);
        
        return await UpdatePhoneNumberConfirmation(profile, phoneNumber);
    }

    [HttpPatch("confirm-phone-number/{confirmationToken}")]
    public async Task<IActionResult> ConfirmPhoneNumber([FromHeader] string accountId, [FromRoute] string confirmationToken) {
        _logger.Log(new LoggerBinding<ProfileController> { Location = nameof(ConfirmPhoneNumber) });
        
        var profile = await _profileService.GetProfileByAccountId(accountId);
        if (profile is null) return new ErrorResponse();
        
        if (profile.PhoneNumberConfirmed) return new ErrorResponse(HttpStatusCode.Continue);
        if (!Equals(profile.PhoneNumberToken, confirmationToken)) return new ErrorResponse(HttpStatusCode.Forbidden);
        if (profile.PhoneNumberTokenTimestamp!.Value.Compute(_haloConfigs.PhoneTokenValidityDuration, _haloConfigs.PhoneTokenValidityDurationUnit) > DateTime.UtcNow)
            return new ErrorResponse(HttpStatusCode.Gone);

        profile.PhoneNumberToken = null;
        profile.PhoneNumberTokenTimestamp = null;
        profile.PhoneNumberConfirmed = true;
        
        var profileUpdated = await _profileService.UpdateProfile(profile);
        return !profileUpdated.HasValue || !profileUpdated.Value ? new ErrorResponse() : new SuccessResponse();
    }

    [HttpGet("details")]
    [ServiceFilter(typeof(AccountAndProfileAssociatedAuthorize))]
    public async Task<IActionResult> GetProfileDetails([FromHeader] string profileId) {
        _logger.Log(new LoggerBinding<ProfileController> { Location = nameof(GetProfileDetails) });
        
        var profileDetails = await _profileService.GetProfileDetails(profileId);
        return profileDetails is null ? new ErrorResponse() : new SuccessResponse(profileDetails);
    }
    
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [ServiceFilter(typeof(AccountAndProfileAssociatedAuthorize))]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateProfile([FromHeader] string profileId, [FromBody] ProfileUpdateData profileData) {
        _logger.Log(new LoggerBinding<ProfileController> { Location = nameof(UpdateProfile) });

        var errors = await profileData.VerifyProfileUpdateData(_profileUpdateDataHandler);
        if (errors is null) return new ErrorResponse();
        if (errors.Count != 0) return new ErrorResponse(HttpStatusCode.BadRequest, errors);

        var profile = await _profileService.GetProfile(profileId);
        if (profile is null) return new ErrorResponse();

        switch (profileData.FieldName) {
            case nameof(Profile.GivenName):
                profile.GivenName = profileData.StrValue;
                break;
            case nameof(Profile.MiddleName):
                profile.MiddleName = profileData.StrValue;
                break;
            case nameof(Profile.LastName):
                profile.LastName = profileData.StrValue;
                break;
            case nameof(Profile.FullName):
                profile.FullName = profileData.StrValue;
                break;
            case nameof(Profile.NickName):
                profile.NickName = profileData.StrValue;
                break;
            case nameof(Profile.DateOfBirth):
                profile.DateOfBirth = profileData.StrValue?.ToDateTime();
                break;
            case nameof(Profile.Gender):
                profile.Gender = (byte)profileData.IntValue!.Value;
                break;
            case nameof(Profile.Ethnicity):
                profile.Ethnicity = (byte)profileData.IntValue!.Value;
                break;
            case nameof(Profile.Company):
                profile.Company = profileData.StrValue;
                break;
            case nameof(Profile.JobTitle):
                profile.JobTitle = profileData.StrValue;
                break;
            case nameof(Profile.Websites):
                var websites = profileData.IntValueMaps?.Select(entry => new ProfileLinkVM {
                    LinkType = (Enums.SocialMedia)entry.Key,
                    LinkHref = entry.Value,
                }).ToArray();
                profile.Websites = websites is null ? null : JsonConvert.SerializeObject(websites);
                break;
            case nameof(Profile.Interests):
                profile.Interests = profileData.StrValues is null ? null : JsonConvert.SerializeObject(profileData.StrValues);
                break;
        }
        
        var profileUpdated = await _profileService.UpdateProfile(profile);
        return !profileUpdated.HasValue || !profileUpdated.Value ? new ErrorResponse() : new SuccessResponse();
    }

    [ServiceFilter(typeof(AccountAndProfileAssociatedAuthorize))]
    [HttpPost("save-profile-photo")]
    public async Task<IActionResult> SetOrChangeProfilePhoto([FromHeader] string profileId, [FromForm] ProfilePhotoUpload photoUpload) {
        _logger.Log(new LoggerBinding<ProfileController> { Location = nameof(SetOrChangeProfilePhoto) });

        var errors = photoUpload.VerifyData();
        if (errors.Count != 0) return new ErrorResponse(HttpStatusCode.BadRequest, errors);
        
        var profile = await _profileService.GetProfile(profileId);
        if (profile is null) return new ErrorResponse();

        var uploadResult = photoUpload.IsAvatar
            ? await _profilePhotoService.UploadAvatar(profileId, photoUpload.Photo, profile.AvatarName)
            : await _profilePhotoService.UploadCover(profileId, photoUpload.Photo, profile.CoverName);
        
        if (uploadResult is null) return new ErrorResponse();
        if (!uploadResult.IsSuccess) return new ErrorResponse(HttpStatusCode.Gone, new { message = uploadResult.Message });

        if (photoUpload.IsAvatar) profile.AvatarName = uploadResult.FileName;
        else profile.CoverName = uploadResult.FileName;
        
        var profileUpdateResult = await _profileService.UpdateProfile(profile);
        if (profileUpdateResult.HasValue && profileUpdateResult.Value) return new SuccessResponse(new { fileName = uploadResult.FileName });
        
        _ = await _profilePhotoService.DeletePhoto(profileId, uploadResult.FileName!, photoUpload.IsAvatar);
        return new ErrorResponse(HttpStatusCode.Conflict);
    }

    [ServiceFilter(typeof(AccountAndProfileAssociatedAuthorize))]
    [HttpPatch("delete-profile-photo/{isAvatar:int}")]
    public async Task<IActionResult> RemoveProfilePhoto([FromHeader] string profileId, [FromRoute] int isAvatar) {
        _logger.Log(new LoggerBinding<ProfileController> { Location = nameof(RemoveProfilePhoto) });
        
        var profile = await _profileService.GetProfile(profileId);
        if (profile is null) return new ErrorResponse();
        
        var fileName = isAvatar != 1 ? profile.CoverName : profile.AvatarName;
        if (!fileName.IsString()) return new ErrorResponse(HttpStatusCode.NotFound);
        
        if (isAvatar != 1) profile.CoverName = null;
        else profile.AvatarName = null;

        await _contextService.StartTransaction();
        
        var profileUpdateResult = await _profileService.UpdateProfile(profile);
        if (!profileUpdateResult.HasValue || !profileUpdateResult.Value) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }
        
        var deleteFileResult = await _profilePhotoService.DeletePhoto(profileId, fileName!, isAvatar == 1);
        if (deleteFileResult.HasValue && deleteFileResult.Value) {
            await _contextService.ConfirmTransaction();
            return new SuccessResponse();
        }
        
        await _contextService.RevertTransaction();
        return new ErrorResponse();
    }

    private async Task<IActionResult> UpdatePhoneNumberConfirmation(Profile profile, RegionalizedPhoneNumber? phoneNumber = null) {
        _logger.Log(new LoggerBinding<ProfileController> { IsPrivate = true, Location = nameof(UpdatePhoneNumberConfirmation) });

        if (phoneNumber is not null) {
            profile.PhoneNumber = JsonConvert.SerializeObject(phoneNumber);
            profile.PhoneNumberConfirmed = false;
        }
        
        profile.PhoneNumberToken = StringHelpers.GenerateRandomString(
            NumberHelpers.GetRandomNumberInRangeInclusive(_haloConfigs.PhoneTokenMinLength, _haloConfigs.PhoneTokenMaxLength)
        );
        profile.PhoneNumberTokenTimestamp = DateTime.UtcNow;

        await _contextService.StartTransaction();
        var profileUpdated = await _profileService.UpdateProfile(profile);
        if (!profileUpdated.HasValue || !profileUpdated.Value) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        var smsContent = _haloConfigs.PhoneNumberConfirmationSmsContent
            .Replace("CLIENT_BASE_URI", _haloConfigs.ClientBaseUri)
            .Replace("CLIENT_APPLICATION_NAME", Constants.ProjectName)
            .Replace("USERNAME", profile.NickName ?? profile.FullName ?? $"{profile.GivenName} {profile.MiddleName} {profile.LastName}")
            .Replace("CONFIRMATION_TOKEN", profile.PhoneNumberToken)
            .Replace("VALIDITY_DURATION", $"{_haloConfigs.PhoneTokenValidityDuration} {_haloConfigs.PhoneTokenValidityDurationUnit}s");

        var smsBinding = new SingleSmsBinding {
            SmsContent = smsContent,
            Receivers = [phoneNumber?.ToPhoneNumber() ?? RegionalizedPhoneNumber.Deserialize(profile.PhoneNumber!)!.ToPhoneNumber(),]
        };
        
        var smsResult = await _smsService.SendSingleSms(smsBinding);
        if (smsResult is null || smsResult.Length != 0) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }
        
        await _contextService.ConfirmTransaction();
        return new SuccessResponse();
    }
}