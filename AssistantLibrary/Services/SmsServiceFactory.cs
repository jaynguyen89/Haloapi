using AssistantLibrary.Interfaces;
using AssistantLibrary.Interfaces.IServiceFactory;
using HelperLibrary.Shared;
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
        _activeSmsServiceName = _assistantConfigs.ServiceFactorySettings.ActiveSmsService;
    }

    public ISmsService GetActiveSmsService() {
        var serviceType = Type.GetType(_activeSmsServiceName);
        if (serviceType is null) throw new ServiceNotFoundByNameException(_activeSmsServiceName);
        
        return (ISmsService)Activator.CreateInstance(serviceType, _ecosystem, _logger, _configuration)!;
    }
}