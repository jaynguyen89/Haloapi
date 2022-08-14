using System.Net;
using Halogen.Attributes;
using Halogen.Bindings.ApiBindings;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;

namespace Halogen.Controllers; 

[ApiController]
[Route("profile")]
public sealed class ProfileController: AppController {
    
    private readonly IContextService _contextService;
    private readonly IProfileService _profileService;

    public ProfileController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        IContextService contextService,
        IProfileService profileService
    ) : base(ecosystem, logger, configuration) {
        _contextService = contextService;
        _profileService = profileService;
    }

    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpGet("check-phone-number-availability/{phoneNumber}")]
    public async Task<JsonResult> IsPhoneNumberAvailable([FromRoute] string phoneNumber) {
        _logger.Log(new LoggerBinding<ProfileController> { Location = nameof(IsPhoneNumberAvailable) });
        
        if (!phoneNumber.IsString()) return new JsonResult(new StatusCodeResult((int)HttpStatusCode.BadRequest));

        var isPhoneNumberAvailable = await _profileService.IsPhoneNumberAvailableForNewAccount(phoneNumber);
        return !isPhoneNumberAvailable.HasValue
            ? new JsonResult(new StatusCodeResult((int)HttpStatusCode.InternalServerError))
            : new JsonResult(new ClientResponse { Result = Enums.ApiResult.Success, Data = isPhoneNumberAvailable.Value });
    }
}