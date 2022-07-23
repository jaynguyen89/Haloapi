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

    public SmsServiceFactory(
        IEcosystem ecosystem,
        ILoggerService logger,
        IOptions<AssistantLibraryOptions> options
    ): base(ecosystem, logger, options) {
        _ecosystem = ecosystem;
        
        _activeSmsServiceName = ecosystem.GetEnvironment() switch {
            Constants.Development => options.Value.Dev.ServiceFactorySettings.ActiveSmsService,
            Constants.Staging => options.Value.Stg.ServiceFactorySettings.ActiveSmsService,
            _ => options.Value.Prod.ServiceFactorySettings.ActiveSmsService
        };
    }

    // Todo: improve this by dynamically get the service using ServiceLocator
    public ISmsService GetActiveSmsService() {
        if (!_activeSmsServiceName.Equals(nameof(ClickatellSmsHttpService)))
            return new ClickatellSmsRestService(_ecosystem, _logger, _options);
        
        var (httpEndpoint, apiKey) = _environment switch {
            Constants.Development => (
                _options.Value.Dev.ClickatellHttpSettings.HttpEndpoint,
                _options.Value.Dev.ClickatellHttpSettings.ApiKey
            ),
            Constants.Staging => (
                _options.Value.Stg.ClickatellHttpSettings.HttpEndpoint,
                _options.Value.Stg.ClickatellHttpSettings.ApiKey
            ),
            _ => (
                _options.Value.Prod.ClickatellHttpSettings.HttpEndpoint,
                _options.Value.Prod.ClickatellHttpSettings.ApiKey
            )
        };

        var clickatellBaseUrl = $"{httpEndpoint}?{nameof(apiKey)}={apiKey}";
        return new ClickatellSmsHttpService(_ecosystem, _logger, _options, clickatellBaseUrl);

    }
}