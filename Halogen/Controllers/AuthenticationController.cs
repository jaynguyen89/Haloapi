using Halogen.Bindings.ApiBindings;
using Halogen.Parsers;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;
using Halogen.Attributes;

namespace Halogen.Controllers; 

[ApiController]
[Route("authentication")]
internal sealed class AuthenticationController: AppController {

    internal AuthenticationController(
        IEcosystem ecosystem,
        ILoggerService logger,
        HalogenOptions options
    ) : base(ecosystem, logger, options) { }

    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPost("register")]
    public async Task<JsonResult> RegisterAccount(RegistrationData registrationData) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(RegisterAccount) });

        var errors = await registrationData.VerifyRegistrationData();
        if (errors.Any()) return new JsonResult(new ClientResponse { Result = Enums.ApiResult.FAILED, Data = errors });

        return null;
    }
}
