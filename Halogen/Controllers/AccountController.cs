using System.Net;
using Halogen.Attributes;
using Halogen.Bindings.ViewModels;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;

namespace Halogen.Controllers; 

[ApiController]
[Route("account")]
public sealed class AccountController: AppController {
    
    private readonly IContextService _contextService;
    private readonly IAccountService _accountService;

    public AccountController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        IContextService contextService,
        IAccountService accountService
    ) : base(ecosystem, logger, configuration) {
        _contextService = contextService;
        _accountService = accountService;
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