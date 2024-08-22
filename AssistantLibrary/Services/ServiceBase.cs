using AssistantLibrary.Bindings;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Configuration;

namespace AssistantLibrary.Services; 

public class ServiceBase {

    protected readonly string _environment;
    protected readonly ILoggerService _logger;
    protected readonly IConfiguration _configuration;
    protected readonly AssistantConfigs _assistantConfigs;

    internal protected ServiceBase() { }

    internal ServiceBase(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ) {
        _environment = ecosystem.GetEnvironment();
        _logger = logger;
        _configuration = configuration;
        var assistantConfigProvider = new AssistantConfigProvider(ecosystem, logger, configuration);
        _assistantConfigs = assistantConfigProvider.GetAssistantConfigs();
    }
}