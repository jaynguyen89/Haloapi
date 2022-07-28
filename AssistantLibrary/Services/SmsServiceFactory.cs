using AssistantLibrary.Interfaces;
using AssistantLibrary.Interfaces.IServiceFactory;
using AssistantLibrary.Services.ServiceFactory;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Options;

namespace AssistantLibrary.Services; 

public sealed class SmsServiceFactory: ServiceBase, ISmsServiceFactory {

    private readonly IEcosystem _ecosystem;
    private readonly string _activeSmsServiceName;
    private readonly IOptions<AssistantLibraryOptions> _iOptions;

    public SmsServiceFactory(
        IEcosystem ecosystem,
        ILoggerService logger,
        IOptions<AssistantLibraryOptions> options
    ): base(ecosystem, logger, options) {
        _ecosystem = ecosystem;
        _iOptions = options;
        
        _activeSmsServiceName = ecosystem.GetEnvironment() switch {
            Constants.Development => options.Value.Dev.ServiceFactorySettings.ActiveSmsService,
            Constants.Staging => options.Value.Stg.ServiceFactorySettings.ActiveSmsService,
            Constants.Production => options.Value.Prod.ServiceFactorySettings.ActiveSmsService,
            _ => options.Value.Loc.ServiceFactorySettings.ActiveSmsService
        };
    }

    // Todo: improve this by dynamically get the service using ServiceLocator
    public ISmsService GetActiveSmsService() {
        if (!_activeSmsServiceName.Equals(nameof(ClickatellSmsHttpService)))
            return new ClickatellSmsRestService(_ecosystem, _logger, _iOptions);
        
        var (httpEndpoint, apiKey) = _environment switch {
            Constants.Development => (
                _options.Dev.ClickatellHttpSettings.HttpEndpoint,
                _options.Dev.ClickatellHttpSettings.ApiKey
            ),
            Constants.Staging => (
                _options.Stg.ClickatellHttpSettings.HttpEndpoint,
                _options.Stg.ClickatellHttpSettings.ApiKey
            ),
            Constants.Production => (
                _options.Prod.ClickatellHttpSettings.HttpEndpoint,
                _options.Prod.ClickatellHttpSettings.ApiKey
            ),
            _ => (
                _options.Loc.ClickatellHttpSettings.HttpEndpoint,
                _options.Loc.ClickatellHttpSettings.ApiKey
            )
        };

        var clickatellBaseUrl = $"{httpEndpoint}?{nameof(apiKey)}={apiKey}";
        return new ClickatellSmsHttpService(_ecosystem, _logger, _iOptions, clickatellBaseUrl);
    }
}