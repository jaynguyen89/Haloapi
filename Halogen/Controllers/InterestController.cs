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
[Route("interests")]
[ServiceFilter(typeof(AuthenticatedAuthorize))]
[ServiceFilter(typeof(TwoFactorAuthorize))]
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

    /// <summary>
    /// For guests. To get the list of all predefined interests, which is used for the input of interests in user's Profile.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     GET /interests/all
    /// </code>
    /// -->
    /// </remarks>
    /// <response code="200">
    /// Successful request with data as follows:
    /// <code>
    /// InterestVM {
    ///     id: string,
    ///     name: string,
    ///     description?: string,
    ///     parent?: InterestVM,
    /// }
    /// </code>
    /// </response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllInterests() {
        _logger.Log(new LoggerBinding<InterestController> { Location = nameof(GetAllInterests) });

        var interests = await _interestService.GetAllInterests();
        return interests is null ? new ErrorResponse() : new SuccessResponse(interests);
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetInterestsList() {
        _logger.Log(new LoggerBinding<InterestController> { Location = nameof(GetInterestsList) });
        
        var interests = await _interestService.GetAllInterestsAsList();
        return interests is null ? new ErrorResponse() : new SuccessResponse(interests);
    }

    [ServiceFilter(typeof(AccountAndProfileAssociatedAuthorize))]
    [HttpGet("profile-interests")]
    public async Task<IActionResult> GetProfileInterests([FromHeader] string profileId) {
        _logger.Log(new LoggerBinding<InterestController> { Location = nameof(GetProfileInterests) });

        var interests = await _interestService.GetProfileInterests(profileId);
        return interests is null ? new ErrorResponse() : new SuccessResponse(interests);
    }
}