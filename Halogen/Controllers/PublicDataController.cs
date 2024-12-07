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

    /// <summary>
    /// For guest. To get the configuration settings from server side, which will be used to render UI content formats.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     GET /enums
    /// </code>
    /// -->
    /// </remarks>
    /// <response code="200">
    /// Successful request with data as follows:
    /// <code>
    /// {
    ///     environment: string,
    ///     secretCodeEnabled: boolean,
    ///     secretCodeLength: number,
    ///     dateFormats: Array:{
    ///         index: number,
    ///         display: string,
    ///     },
    ///     timeFormats: Array:{
    ///         index: number,
    ///         display: string,
    ///     },
    ///     numberFormats: Array:{
    ///         index: number,
    ///         display: string,
    ///     },
    ///     genders: Array:{
    ///         index: number,
    ///         display: string,
    ///     },
    ///     languages: Array:{
    ///         code: string,
    ///         display: string,
    ///     },
    ///     themes: Array:{
    ///         index: number,
    ///         display: string,
    ///     },
    ///     nameFormats: Array:{
    ///         index: number,
    ///         display: string,
    ///     },
    ///     birthFormats: Array:{
    ///         index: number,
    ///         display: string,
    ///     },
    ///     phoneNumberFormats: Array:{
    ///         index: number,
    ///         display: string,
    ///     },
    ///     unitSystems: Array:{
    ///         index: number,
    ///         display: string,
    ///     },
    ///     careerFormats: Array:{
    ///         index: number,
    ///         display: string,
    ///     },
    ///     visibilityFormats: Array:{
    ///         index: number,
    ///         display: string,
    ///     },
    ///     countries: Array:{
    ///         name: string,
    ///         isoCode2Char: string,
    ///         isoCode3Char: string,
    ///         telephoneCode: string,
    ///     },
    ///     supportedSocialAccounts: Array:string,
    /// }
    /// </code>
    /// </response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [HttpGet("enums")]
    public async Task<IActionResult> GetPublicData() {
        _logger.Log(new LoggerBinding<PublicDataController> { Location = nameof(GetPublicData) });

        var localities = await _localityService.GetLocalitiesForPublicData();
        if (localities is null) return new ErrorResponse();

        var publicData = new PublicData {
            Environment = _environment,
            SecretCodeEnabled = _haloConfigs.EnableSecretCode,
            SecretCodeLength = Constants.SecretCodeLength,
            DateFormats = EnumHelpers.ToArrayWithValueAttribute<Enums.DateFormat>(),
            TimeFormats = EnumHelpers.ToArrayWithValueAttribute<Enums.TimeFormat>(),
            NumberFormats = EnumHelpers.ToArrayWithValueAttribute<Enums.NumberFormat>(),
            Genders = EnumHelpers.ToArrayWithValueAttribute<Enums.Gender>(),
            Languages = EnumHelpers.ToArrayWithCompositeAttribute<Enums.Language>(),
            Themes = EnumHelpers.ToArrayWithValueAttribute<Enums.ApplicationTheme>(),
            NameFormats = EnumHelpers.ToArrayWithValueAttribute<Enums.NameFormat>(),
            BirthFormats = EnumHelpers.ToArrayWithValueAttribute<Enums.BirthFormat>(),
            PhoneNumberFormats = EnumHelpers.ToArrayWithValueAttribute<Enums.PhoneNumberFormat>(),
            UnitSystems = EnumHelpers.ToArrayWithValueAttribute<Enums.UnitSystem>(),
            CareerFormats = EnumHelpers.ToArrayWithValueAttribute<Enums.CareerFormat>(),
            VisibilityFormats = EnumHelpers.ToArrayWithValueAttribute<Enums.Visibility>(),
            Countries = localities.Select(locality => (PublicData.CountryData)locality).ToArray(),
            SupportedSocialAccounts = _haloConfigs.SupportedSocialAccountForRegistration
                .Select(x => x.GetValue() ?? string.Empty)
                .Where(x => x.IsString())
                .ToArray(),
        };
        
        return new SuccessResponse(publicData);
    }
}