using Halogen.Bindings;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings.ApiBindings;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;

namespace Halogen.Controllers; 

[ApiController]
[Route("profile")]
[AutoValidateAntiforgeryToken]
public sealed class ProfileController: AppController {
    
    private readonly IContextService _contextService;
    private readonly IProfileService _profileService;

    public ProfileController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        IHaloServiceFactory haloServiceFactory,
        IHaloConfigProvider haloConfigProvider
    ) : base(ecosystem, logger, configuration, haloConfigProvider.GetHalogenConfigs()) {
        _contextService = haloServiceFactory.GetService<ContextService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<ProfileController>(nameof(ContextService));
        _profileService = haloServiceFactory.GetService<ProfileService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<ProfileController>(nameof(ProfileService));
    }

    [HttpGet("get-phone-number-credential")]
    public async Task<IActionResult> GetPhoneNumberCredential([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<ProfileController> { Location = nameof(GetPhoneNumberCredential) });
    }

    [HttpPatch("confirm-phone-number")]
    public async Task<IActionResult> ConfirmPhoneNumber([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<ProfileController> { Location = nameof(ConfirmPhoneNumber) });
    }

    [HttpPatch("replace-phone-number")]
    public async Task<IActionResult> ReplacePhoneNumber([FromHeader] string accountId, [FromBody] RegionalizedPhoneNumber phoneNumber) {
        _logger.Log(new LoggerBinding<ProfileController> { Location = nameof(ReplacePhoneNumber) });
    }

    [HttpGet("get-details")]
    public async Task<IActionResult> GetProfileDetails([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<ProfileController> { Location = nameof(GetProfileDetails) });
    }
    
    [HttpPut("update")]
    public async Task<IActionResult> UpdateProfile([FromHeader] string accountId, [FromBody] ProfileUpdateData profileData) {
        _logger.Log(new LoggerBinding<ProfileController> { Location = nameof(UpdateProfile) });
    }
}