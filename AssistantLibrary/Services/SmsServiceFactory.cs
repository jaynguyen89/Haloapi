using AssistantLibrary.Interfaces;
using AssistantLibrary.Interfaces.IServiceFactory;
using AssistantLibrary.Services.ServiceFactory;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Configuration;

namespace AssistantLibrary.Services; 

public sealed class SmsServiceFactory: ServiceBase, ISmsServiceFactory {

    private readonly IEcosystem _ecosystem;
    private readonly string _activeSmsServiceName;

    public SmsServiceFactory(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ): base(ecosystem, logger, configuration) {
        _ecosystem = ecosystem;
        _activeSmsServiceName = _configuration.AsEnumerable().Single(x => x.Key.Equals($"{_serviceFactoryBaseOptionKey}{nameof(AssistantLibraryOptions.Local.ServiceFactorySettings.ActiveSmsService)}")).Value;
    }

    // Todo: improve this by dynamically get the service using ServiceLocator
    public ISmsService GetActiveSmsService() {
        if (!_activeSmsServiceName.Equals(nameof(ClickatellSmsHttpService)))
            return new ClickatellSmsRestService(_ecosystem, _logger, _configuration);
        
        var (httpEndpoint, apiKey) = (
            _configuration.AsEnumerable().Single(x => x.Key.Equals($"{_clickatellBaseOptionKey}{nameof(AssistantLibraryOptions.Local.ClickatellHttpSettings.HttpEndpoint)}")).Value,
            _configuration.AsEnumerable().Single(x => x.Key.Equals($"{_clickatellBaseOptionKey}{nameof(AssistantLibraryOptions.Local.ClickatellHttpSettings.ApiKey)}")).Value
        );

        var clickatellBaseUrl = $"{httpEndpoint}?{nameof(apiKey)}={apiKey}";
        return new ClickatellSmsHttpService(_ecosystem, _logger, _configuration, clickatellBaseUrl);
    }
}