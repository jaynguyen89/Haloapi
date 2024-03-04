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
[Route("account")]
[AutoValidateAntiforgeryToken]
public sealed class AccountController: AppController {
    
    private readonly IContextService _contextService;
    private readonly IAccountService _accountService;

    public AccountController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        IHaloServiceFactory haloServiceFactory,
        IHaloConfigProvider haloConfigProvider
    ) : base(ecosystem, logger, configuration, haloConfigProvider.GetHalogenConfigs()) {
        _contextService = haloServiceFactory.GetService<ContextService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AccountController>(nameof(ContextService));
        _accountService = haloServiceFactory.GetService<AccountService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AccountController>(nameof(AccountService));
    }

    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpGet("check-email-availability/{emailAddress}")]
    public async Task<IActionResult> IsEmailAddressAvailable([FromRoute] string emailAddress) {
        _logger.Log(new LoggerBinding<AccountController> { Location = nameof(IsEmailAddressAvailable) });

        if (!emailAddress.IsString()) return new ErrorResponse(HttpStatusCode.BadRequest);

        var isEmailAvailable = await _accountService.IsEmailAddressAvailableForNewAccount(emailAddress);
        return !isEmailAvailable.HasValue
            ? new ErrorResponse()
            : new SuccessResponse(new { isEmailAvailable = isEmailAvailable.Value });
    }
}