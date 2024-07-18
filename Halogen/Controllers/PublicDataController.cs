using Halogen.Bindings;
using Halogen.Bindings.ViewModels;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;

namespace Halogen.Controllers;

[ApiController]
[Route("public-data")]
public sealed class PublicDataController: AppController {

    private readonly ILocalityService _localityService;

    public PublicDataController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        IHaloServiceFactory haloServiceFactory,
        IHaloConfigProvider haloConfigProvider
    ) : base(ecosystem, logger, configuration, haloConfigProvider.GetHalogenConfigs()) {
        _localityService = haloServiceFactory.GetService<LocalityService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<PublicDataController>(nameof(LocalityService));
    }

    [HttpGet("enums")]
    public async Task<IActionResult> GetPublicData() {
        _logger.Log(new LoggerBinding<PublicDataController> { Location = nameof(GetPublicData) });

        var localities = await _localityService.GetLocalitiesForPublicData();
        if (localities is null) return new ErrorResponse();

        var publicData = new PublicData {
            Environment = _environment,
            EnableSecretCode = _haloConfigs.EnableSecretCode,
            SecretCodeLength = Constants.SecretCodeLength,
            DateFormats = EnumHelpers.ToArrayWithValueAttribute<Enums.DateFormat>(),
            TimeFormats = EnumHelpers.ToArrayWithValueAttribute<Enums.TimeFormat>(),
            NumberFormats = EnumHelpers.ToArrayWithValueAttribute<Enums.NumberFormat>(),
            Genders = EnumHelpers.ToArrayWithValueAttribute<Enums.GenderType>(),
            Languages = EnumHelpers.ToArrayWithCompositeAttribute<Enums.Language>(),
            Themes = EnumHelpers.ToArrayWithValueAttribute<Enums.ApplicationTheme>(),
            NameFormats = EnumHelpers.ToArrayWithValueAttribute<Enums.NameFormat>(),
            BirthFormats = EnumHelpers.ToArrayWithValueAttribute<Enums.BirthFormat>(),
            PhoneNumberFormats = EnumHelpers.ToArrayWithValueAttribute<Enums.PhoneNumberFormat>(),
            UnitSystems = EnumHelpers.ToArrayWithValueAttribute<Enums.UnitSystem>(),
            CareerFormats = EnumHelpers.ToArrayWithValueAttribute<Enums.CareerFormat>(),
            VisibilityFormats = EnumHelpers.ToArrayWithValueAttribute<Enums.Visibility>(),
            Countries = localities.Select(locality => (PublicData.CountryData)locality).ToArray(),
        };
        
        return new SuccessResponse(publicData);
    }
}