using AssistantLibrary.Interfaces;
using AssistantLibrary.Interfaces.IServiceFactory;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Configuration;

namespace AssistantLibrary.Services; 

public sealed class SmsServiceFactory: ServiceBase, ISmsServiceFactory {

    private readonly IEcosystem _ecosystem;
    private readonly string _activeSmsServiceName;

    private readonly Lazy<Dictionary<string, object>> _services = new(() => new Dictionary<string, object>());

    public SmsServiceFactory(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ): base(ecosystem, logger, configuration) {
        _ecosystem = ecosystem;
        _activeSmsServiceName = _assistantConfigs.ServiceFactorySettings.ActiveSmsService;
    }

    public ISmsService GetActiveSmsService() {
        if (_services.Value.ContainsKey(_activeSmsServiceName)) return (ISmsService)_services.Value!.GetDictionaryValue(_activeSmsServiceName)!;
        
        var serviceType = Type.GetType(_activeSmsServiceName);
        if (serviceType is null) throw new ServiceNotFoundByNameException(_activeSmsServiceName);
        
        var service = (ISmsService)Activator.CreateInstance(serviceType, _ecosystem, _logger, _configuration)!;
        
        _services.Value.Add(_activeSmsServiceName, service);
        return service;
    }
}