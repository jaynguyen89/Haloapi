﻿using Halogen.Bindings;
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
            DateFormats = EnumHelpers.ToDictionaryWithValueAttribute<Enums.DateFormat>(),
            TimeFormats = EnumHelpers.ToDictionaryWithValueAttribute<Enums.TimeFormat>(),
            NumberFormats = EnumHelpers.ToDictionaryWithValueAttribute<Enums.NumberFormat>(),
            Genders = EnumHelpers.ToDictionaryWithValueAttribute<Enums.GenderType>(),
            Languages = EnumHelpers.ToDictionaryWithCompositeValueAttribute<Enums.Language>(),
            Themes = EnumHelpers.ToDictionaryWithValueAttribute<Enums.ApplicationTheme>(),
            NameFormats = EnumHelpers.ToDictionaryWithValueAttribute<Enums.NameFormat>(),
            BirthFormats = EnumHelpers.ToDictionaryWithValueAttribute<Enums.BirthFormat>(),
            PhoneNumberFormats = EnumHelpers.ToDictionaryWithValueAttribute<Enums.PhoneNumberFormat>(),
            UnitSystems = EnumHelpers.ToDictionaryWithValueAttribute<Enums.UnitSystem>(),
            CareerFormats = EnumHelpers.ToDictionaryWithValueAttribute<Enums.CareerFormat>(),
            VisibilityFormats = EnumHelpers.ToDictionaryWithValueAttribute<Enums.Visibility>(),
            Countries = localities.Select(locality => (PublicData.CountryData)locality).ToArray(),
        };
        
        return new SuccessResponse(publicData);
    }
}