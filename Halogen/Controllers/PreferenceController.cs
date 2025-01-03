﻿using System.Net;
using AssistantLibrary;
using Halogen.Attributes;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings;
using Halogen.Bindings.ApiBindings;
using Halogen.Bindings.ServiceBindings;
using Halogen.Bindings.ViewModels;
using Halogen.Services.AppServices.Interfaces;
using Halogen.Services.AppServices.Services;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Preference = Halogen.DbModels.Preference;

namespace Halogen.Controllers;

[ApiController]
[Route("preferences")]
[AutoValidateAntiforgeryToken]
[ServiceFilter(typeof(AuthenticatedAuthorize))]
[ServiceFilter(typeof(TwoFactorAuthorize))]
public sealed class PreferenceController: AppController {
    
    private readonly ICacheService _cacheService;
    private readonly IPreferenceService _preferenceService;
    private readonly PreferenceUpdateDataHandler _privacyPolicyHandler;

    public PreferenceController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        IHaloConfigProvider haloConfigProvider,
        IHaloServiceFactory haloServiceFactory,
        IAssistantServiceFactory assistantServiceFactory,
        PreferenceUpdateDataHandler privacyPolicyHandler
    ): base(ecosystem, logger, configuration, haloConfigProvider.GetHalogenConfigs()) {
        _cacheService = assistantServiceFactory.GetService<CacheServiceFactory>()?.GetActiveCacheService() ?? throw new HaloArgumentNullException<AccountController>(nameof(CacheServiceFactory));
        _preferenceService = haloServiceFactory.GetService<PreferenceService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AccountController>(nameof(PreferenceService));
        _privacyPolicyHandler = privacyPolicyHandler;
    }

    [HttpGet("get-preference-settings")]
    public async Task<IActionResult> GetPreferenceSettings([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<PreferenceController> { Location = nameof(GetPreferenceSettings) });
        
        var preferences = await _preferenceService.GetPreferenceSettings(accountId);
        return preferences is null ? new ErrorResponse() : new SuccessResponse(preferences);
    }

    [HttpGet("get-privacy-settings")]
    public async Task<IActionResult> GetPrivacySettings([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<PreferenceController> { Location = nameof(GetPrivacySettings) });

        var privacySettings = await _cacheService.GetCacheEntry<PrivacyVM>($"{nameof(PrivacyVM)}{Constants.Hyphen}{accountId}")
                              ?? await _preferenceService.GetPrivacySettings(accountId);

        if (privacySettings is null) return new ErrorResponse();
        
        await _cacheService.InsertCacheEntry(new MemoryCacheEntry {
            Key = $"{nameof(PrivacyVM)}{Constants.Hyphen}{accountId}",
            Value = privacySettings,
        });

        return new SuccessResponse(privacySettings);

    }

    [HttpPatch("update-preference")]
    public async Task<IActionResult> UpdatePreferenceAndPrivacy([FromHeader] string accountId, [FromBody] PreferenceUpdateData preferenceData) {
        _logger.Log(new LoggerBinding<PreferenceController> { Location = nameof(UpdatePreferenceAndPrivacy) });

        var errors = await preferenceData.VerifyData(_privacyPolicyHandler);
        if (errors is null) return new ErrorResponse();
        if (errors.Count != 0) return new ErrorResponse(HttpStatusCode.BadRequest, errors);

        var preference = await _preferenceService.GetPreference(accountId);
        if (preference is null) return new ErrorResponse();

        try {
            switch (preferenceData.FieldName) {
                case nameof(Preference.ApplicationTheme):
                    preference.ApplicationTheme = preferenceData.ByteValue;
                    break;
                case nameof(Preference.ApplicationLanguage):
                    preference.ApplicationLanguage = preferenceData.ByteValue;
                    break;
                case nameof(Preference.DataFormat):
                    var dataFormat = JsonConvert.DeserializeObject<List<DataFormat>>(preference.DataFormat!);

                    switch (preferenceData.PropertyName) {
                        case nameof(DataFormat.DataType.Date):
                            dataFormat!.RemoveAt(dataFormat.FindIndex(x => x.DtType == DataFormat.DataType.Date));
                            dataFormat.Add(new DataFormat {
                                DtType = DataFormat.DataType.Date,
                                Format = preferenceData.ByteValue,
                            });
                            break;
                        case nameof(DataFormat.DataType.Time):
                            dataFormat!.RemoveAt(dataFormat.FindIndex(x => x.DtType == DataFormat.DataType.Time));
                            dataFormat.Add(new DataFormat {
                                DtType = DataFormat.DataType.Time,
                                Format = preferenceData.ByteValue,
                            });
                            break;
                        case nameof(DataFormat.DataType.Number):
                            dataFormat!.RemoveAt(dataFormat.FindIndex(x => x.DtType == DataFormat.DataType.Number));
                            dataFormat.Add(new DataFormat {
                                DtType = DataFormat.DataType.Number,
                                Format = preferenceData.ByteValue,
                            });
                            break;
                        case nameof(DataFormat.DataType.UnitSystem):
                            dataFormat!.RemoveAt(dataFormat.FindIndex(x => x.DtType == DataFormat.DataType.UnitSystem));
                            dataFormat.Add(new DataFormat {
                                DtType = DataFormat.DataType.UnitSystem,
                                Format = preferenceData.ByteValue,
                            });
                            break;
                    }

                    preference.DataFormat = JsonConvert.SerializeObject(dataFormat);
                    break;
                case nameof(PrivacyPreference.ProfilePreference):
                    var settings = JsonConvert.DeserializeObject<PrivacyPreference>(preference.Privacy);

                    switch (preferenceData.PropertyName) {
                        case nameof(ProfilePolicy.HiddenToSearchEngines):
                            settings!.ProfilePreference.HiddenToSearchEngines = preferenceData.BoolValue;
                            break;
                        case nameof(ProfilePolicy.ViewableByStrangers):
                            settings!.ProfilePreference.ViewableByStrangers = preferenceData.BoolValue;
                            break;
                    }

                    preference.Privacy = JsonConvert.SerializeObject(settings);
                    break;
                case nameof(PrivacyPreference.NamePreference):
                    settings = JsonConvert.DeserializeObject<PrivacyPreference>(preference.Privacy);

                    switch (preferenceData.PropertyName) {
                        case nameof(PrivacyPolicy.DataFormat):
                            settings!.NamePreference.DataFormat = preferenceData.ByteValue;
                            break;
                        case nameof(PrivacyPolicy.Visibility):
                            settings!.NamePreference.Visibility = (Enums.Visibility)preferenceData.ByteValue;
                            break;
                        case nameof(PrivacyPolicy.VisibleToIds):
                            settings!.NamePreference.VisibleToIds = preferenceData.StrValues;
                            break;
                    }

                    preference.Privacy = JsonConvert.SerializeObject(settings);
                    break;
                case nameof(PrivacyPreference.BirthPreference):
                    settings = JsonConvert.DeserializeObject<PrivacyPreference>(preference.Privacy);

                    switch (preferenceData.PropertyName) {
                        case nameof(PrivacyPolicy.DataFormat):
                            settings!.BirthPreference.DataFormat = preferenceData.ByteValue;
                            break;
                        case nameof(PrivacyPolicy.Visibility):
                            settings!.BirthPreference.Visibility = (Enums.Visibility)preferenceData.ByteValue;
                            break;
                        case nameof(PrivacyPolicy.VisibleToIds):
                            settings!.BirthPreference.VisibleToIds = preferenceData.StrValues;
                            break;
                    }

                    preference.Privacy = JsonConvert.SerializeObject(settings);
                    break;
                case nameof(PrivacyPreference.CareerPreference):
                    settings = JsonConvert.DeserializeObject<PrivacyPreference>(preference.Privacy);

                    switch (preferenceData.PropertyName) {
                        case nameof(PrivacyPolicy.DataFormat):
                            settings!.CareerPreference.DataFormat = preferenceData.ByteValue;
                            break;
                        case nameof(PrivacyPolicy.Visibility):
                            settings!.CareerPreference.Visibility = (Enums.Visibility)preferenceData.ByteValue;
                            break;
                        case nameof(PrivacyPolicy.VisibleToIds):
                            settings!.CareerPreference.VisibleToIds = preferenceData.StrValues;
                            break;
                    }

                    preference.Privacy = JsonConvert.SerializeObject(settings);
                    break;
                case nameof(PrivacyPreference.PhoneNumberPreference):
                    settings = JsonConvert.DeserializeObject<PrivacyPreference>(preference.Privacy);

                    switch (preferenceData.PropertyName) {
                        case nameof(PrivacyPolicy.DataFormat):
                            settings!.PhoneNumberPreference.DataFormat = preferenceData.ByteValue;
                            break;
                        case nameof(PrivacyPolicy.Visibility):
                            settings!.PhoneNumberPreference.Visibility = (Enums.Visibility)preferenceData.ByteValue;
                            break;
                        case nameof(PrivacyPolicy.VisibleToIds):
                            settings!.PhoneNumberPreference.VisibleToIds = preferenceData.StrValues;
                            break;
                    }

                    preference.Privacy = JsonConvert.SerializeObject(settings);
                    break;
                case nameof(PrivacyPreference.SecurityPreference):
                    settings = JsonConvert.DeserializeObject<PrivacyPreference>(preference.Privacy);
                    
                    var privacySettings = await _cacheService.GetCacheEntry<PrivacyVM>($"{nameof(PrivacyVM)}{Constants.Hyphen}{accountId}")
                                          ?? await _preferenceService.GetPrivacySettings(accountId);
                    if (privacySettings is null) throw new NullReferenceException();

                    switch (preferenceData.PropertyName) {
                        case nameof(SecurityPolicy.NotifyLoginIncidentsOnUntrustedDevices):
                            settings!.SecurityPreference.NotifyLoginIncidentsOnUntrustedDevices = preferenceData.BoolValue;
                            break;
                        case nameof(SecurityPolicy.PrioritizeLoginNotificationsOverEmail):
                            settings!.SecurityPreference.PrioritizeLoginNotificationsOverEmail = privacySettings.SecurityPreference.CanChangeNotifyLoginIncidentsOverEmail
                                ? preferenceData.BoolValue
                                : settings!.SecurityPreference.PrioritizeLoginNotificationsOverEmail;
                            break;
                        case nameof(SecurityPolicy.BlockLoginOnUntrustedDevices):
                            settings!.SecurityPreference.BlockLoginOnUntrustedDevices = privacySettings.SecurityPreference.CanChangeBlockLoginOnUntrustedDevices
                                ? preferenceData.BoolValue
                                : settings!.SecurityPreference.BlockLoginOnUntrustedDevices;
                            break;
                    }

                    preference.Privacy = JsonConvert.SerializeObject(settings);
                    break;
            }
        }
        catch (NullReferenceException e) {
            _logger.Log(new LoggerBinding<PreferenceController> {
                Location = $"{nameof(UpdatePreferenceAndPrivacy)}.{nameof(NullReferenceException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return new ErrorResponse();
        }

        var preferenceUpdated = await _preferenceService.UpdatePreference(preference);
        return !preferenceUpdated.HasValue || !preferenceUpdated.Value ? new ErrorResponse() : new SuccessResponse();
    }
}
