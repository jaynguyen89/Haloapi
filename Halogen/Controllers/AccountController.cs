using Halogen.Bindings;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
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

    [HttpGet("get-authenticated-user-info")]
    public async Task<IActionResult> GetAuthenticatedUserInfo([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<AccountController> { Location = nameof(GetAuthenticatedUserInfo) });
    }

    [HttpGet("get-email-address-credential")]
    public async Task<IActionResult> GetEmailAddressCredential([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<AccountController> { Location = nameof(GetEmailAddressCredential) });
    }

    [HttpPatch("confirm-email-address")]
    public async Task<IActionResult> ConfirmEmailAddress([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<AccountController> { Location = nameof(ConfirmEmailAddress) });
    }

    [HttpPatch("replace-email-address/{emailAddress")]
    public async Task<IActionResult> ReplaceEmailAddress([FromHeader] string accountId, [FromRoute] string emailAddress) {
        _logger.Log(new LoggerBinding<AccountController> { Location = nameof(ReplaceEmailAddress) });
    }
}