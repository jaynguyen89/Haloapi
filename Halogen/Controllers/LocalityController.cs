using Halogen.Attributes;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings;
using Halogen.Bindings.ViewModels;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;

namespace Halogen.Controllers;

[ApiController]
[Route("locality")]
// [AutoValidateAntiforgeryToken]
[ServiceFilter(typeof(AuthenticatedAuthorize))]
[ServiceFilter(typeof(TwoFactorAuthorize))]
public sealed class LocalityController: AppController {
    
    private readonly ILocalityService _localityService;

    public LocalityController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        IHaloConfigProvider haloConfigProvider,
        IHaloServiceFactory haloServiceFactory
    ): base(ecosystem, logger, configuration, haloConfigProvider.GetHalogenConfigs()) {
        _localityService = haloServiceFactory.GetService<LocalityService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AccountController>(nameof(LocalityService));
    }

    [HttpGet("countries/{minimal:int}")]
    public async Task<IActionResult> GetCountries([FromRoute] int minimal = 0) {
        _logger.Log(new LoggerBinding<LocalityController> { Location = nameof(GetCountries) });
        
        var countries = await _localityService.GetCountries(minimal == 1);
        return countries is null ? new ErrorResponse() : new SuccessResponse(countries);
    }

    [HttpGet("country-currencies/{countryId}")]
    public async Task<IActionResult> GetCountryCurrencies([FromRoute] string countryId) {
        _logger.Log(new LoggerBinding<LocalityController> { Location = nameof(GetCountryCurrencies) });
        
        var country = await _localityService.GetCountryById(countryId);
        return country is null
            ? new ErrorResponse()
            : new SuccessResponse(new {
                primary = country.PrimaryCurrencyId,
                secondary = country.SecondaryCurrencyId,
            });
    }

    [HttpGet("localities")]
    public async Task<IActionResult> GetLocalities() {
        _logger.Log(new LoggerBinding<LocalityController> { Location = nameof(GetLocalities) });

        var localities = await _localityService.GetLocalities();
        return localities is null ? new ErrorResponse() : new SuccessResponse(localities);
    }
}