using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings;
using Halogen.Bindings.ViewModels;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;

namespace Halogen.Controllers;

[ApiController]
[Route("interests")]
public sealed class InterestController {

    private readonly ILoggerService _logger;
    private readonly IInterestService _interestService;
    
    public InterestController(
        ILoggerService logger,
        IHaloServiceFactory haloServiceFactory
    ) {
        _logger = logger;
        _interestService = haloServiceFactory.GetService<InterestService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<InterestController>(nameof(InterestService));
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllInterests() {
        _logger.Log(new LoggerBinding<InterestController> { Location = nameof(GetAllInterests) });

        var interests = await _interestService.GetAllInterests();
        return interests is null ? new ErrorResponse() : new SuccessResponse(interests);
    }
}