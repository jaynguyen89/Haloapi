using Halogen.Attributes;
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
[Route("occupations")]
[ServiceFilter(typeof(AuthenticatedAuthorize))]
[ServiceFilter(typeof(TwoFactorAuthorize))]
public sealed class OccupationController {

    private readonly ILoggerService _logger;
    private readonly IOccupationService _occupationService;
    
    public OccupationController(
        ILoggerService logger,
        IHaloServiceFactory haloServiceFactory
    ) {
        _logger = logger;
        _occupationService = haloServiceFactory.GetService<OccupationService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<OccupationController>(nameof(OccupationService));
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllOccupations() {
        _logger.Log(new LoggerBinding<OccupationController> { Location = nameof(GetAllOccupations) });

        var occupations = await _occupationService.GetAllOccupations();
        return occupations is null ? new ErrorResponse() : new SuccessResponse(occupations);
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetOccupationsList() {
        _logger.Log(new LoggerBinding<OccupationController> { Location = nameof(GetOccupationsList) });

        var occupations = await _occupationService.GetAllOccupationsAsList();
        return occupations is null ? new ErrorResponse() : new SuccessResponse(occupations);
    }
}