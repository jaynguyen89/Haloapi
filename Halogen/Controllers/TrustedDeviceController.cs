using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings;
using Halogen.Bindings.ApiBindings;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;

namespace Halogen.Controllers;

[ApiController]
[Route("trusted-device")]
[AutoValidateAntiforgeryToken]
public sealed class TrustedDeviceController: AppController {
    
    private readonly IContextService _contextService;
    private readonly ITrustedDeviceService _trustedDeviceService;

    public TrustedDeviceController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        IHaloConfigProvider haloConfigProvider,
        IHaloServiceFactory haloServiceFactory
    ): base(ecosystem, logger, configuration, haloConfigProvider.GetHalogenConfigs()) {
        _contextService = haloServiceFactory.GetService<ContextService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AccountController>(nameof(ContextService));
        _trustedDeviceService = haloServiceFactory.GetService<TrustedDeviceService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AccountController>(nameof(ContextService));
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<TrustedDeviceController> { Location = nameof(GetAll) });
        // including devices that have been trusted and other devices that were used to login but not trusted
        throw new NotImplementedException();
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddDevice([FromHeader] string accountId, [FromBody] DeviceInformation deviceInfo) {
        _logger.Log(new LoggerBinding<TrustedDeviceController> { Location = nameof(AddDevice) });
        throw new NotImplementedException();
    }

    [HttpDelete("remove/{deviceId}")]
    public async Task<IActionResult> RemoveDevice([FromHeader] string accountId, [FromRoute] string deviceId) {
        _logger.Log(new LoggerBinding<TrustedDeviceController> { Location = nameof(RemoveDevice) });
        throw new NotImplementedException();
    }
}