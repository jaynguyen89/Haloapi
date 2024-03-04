using System.Net;
using Halogen.Attributes;
using Halogen.Bindings;
using Halogen.Bindings.ViewModels;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
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

    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpGet("check-phone-number-availability/{phoneNumber}")]
    public async Task<IActionResult> IsPhoneNumberAvailable([FromRoute] string phoneNumber) {
        _logger.Log(new LoggerBinding<ProfileController> { Location = nameof(IsPhoneNumberAvailable) });
        
        if (!phoneNumber.IsString()) return new ErrorResponse(HttpStatusCode.BadRequest);

        var isPhoneNumberAvailable = await _profileService.IsPhoneNumberAvailableForNewAccount(phoneNumber);
        return !isPhoneNumberAvailable.HasValue
            ? new ErrorResponse()
            : new SuccessResponse(new { isPhoneNumberAvailable = isPhoneNumberAvailable.Value });
    }
}