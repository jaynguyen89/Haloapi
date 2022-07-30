using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Configuration;

namespace AmazonLibrary.Services; 

internal class ServiceBase {

    protected readonly string _environment;
    protected readonly Ecosystem.ServerSettings _serverSettings;
    protected readonly ILoggerService _logger;
    protected readonly IConfiguration _configuration;
    
    internal ServiceBase(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ) {
        _environment = ecosystem.GetEnvironment();
        _serverSettings = ecosystem.GetServerSettings();
        _logger = logger;
        _configuration = configuration;
    }
}