using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;

namespace Halogen.Controllers;

[ApiController]
[Route("preferences")]
[AutoValidateAntiforgeryToken]
public sealed class PreferenceController: AppController {
    
    private readonly IContextService _contextService;
    private readonly IPreferenceService _preferenceService;

    public PreferenceController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        IHaloConfigProvider haloConfigProvider,
        IHaloServiceFactory haloServiceFactory
    ): base(ecosystem, logger, configuration, haloConfigProvider.GetHalogenConfigs()) {
        _contextService = haloServiceFactory.GetService<ContextService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AccountController>(nameof(ContextService));
        _preferenceService = haloServiceFactory.GetService<PreferenceService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AccountController>(nameof(PreferenceService));
    }

    [HttpGet("get-preference-settings")]
    public async Task<IActionResult> GetPreferenceSettings([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<PreferenceController> { Location = nameof(GetPreferenceSettings) });
    }

    [HttpGet("get-privacy-settings")]
    public async Task<IActionResult> GetPrivacySettings([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<PreferenceController> { Location = nameof(GetPrivacySettings) });
    }

    [HttpPatch("update-preference")]
    public async Task<IActionResult> UpdatePreference([FromHeader] string accountId, [FromBody] Preference preference) {
        _logger.Log(new LoggerBinding<PreferenceController> { Location = nameof(UpdatePreference) });
    }

    [HttpPatch("update-privacy")]
    public async Task<IActionResult> UpdatePreferenceSettings([FromHeader] string accountId, [FromBody] Preference preference) {
        _logger.Log(new LoggerBinding<PreferenceController> { Location = nameof(UpdatePreference) });
    }
}