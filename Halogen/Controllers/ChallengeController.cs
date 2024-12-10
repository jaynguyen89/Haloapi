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
[Route("challenges")]
[AutoValidateAntiforgeryToken]
public sealed class ChallengeController: AppController {
    
    private readonly IContextService _contextService;
    private readonly IChallengeService _challengeService;

    public ChallengeController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        IHaloConfigProvider haloConfigProvider,
        IHaloServiceFactory haloServiceFactory
    ): base(ecosystem, logger, configuration, haloConfigProvider.GetHalogenConfigs()) {
        _contextService = haloServiceFactory.GetService<ContextService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AccountController>(nameof(ContextService));
        _challengeService = haloServiceFactory.GetService<ChallengeService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AccountController>(nameof(ChallengeService));
    }

    [HttpGet("questions")]
    public async Task<IActionResult> GetQuestions() {
        _logger.Log(new LoggerBinding<ChallengeController> { Location = nameof(GetQuestions) });
    }

    [HttpGet("responses")]
    public async Task<IActionResult> GetResponses([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<ChallengeController> { Location = nameof(GetResponses) });
    }

    [HttpPut("update-responses")]
    public async Task<IActionResult> UpdateResponses([FromHeader] string accountId, [FromBody] ChallengeResponseData response) {
        _logger.Log(new LoggerBinding<ChallengeController> { Location = nameof(UpdateResponses) });
    }

    [HttpPost("add-responses")]
    public async Task<IActionResult> AddResponses([FromHeader] string accountId, [FromBody] ChallengeResponseData[] response) {
        _logger.Log(new LoggerBinding<ChallengeController> { Location = nameof(AddResponses) });
    }

    [HttpDelete("remove-response/{responseId}")]
    public async Task<IActionResult> RemoveResponses([FromHeader] string accountId, [FromRoute] string responseId) {
        _logger.Log(new LoggerBinding<ChallengeController> { Location = nameof(RemoveResponses) });
    }
}