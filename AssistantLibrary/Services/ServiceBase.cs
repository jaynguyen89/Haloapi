using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Options;

namespace AssistantLibrary.Services; 

public class ServiceBase {

    protected readonly string _environment;
    protected readonly ILoggerService _logger;
    protected readonly IOptions<AssistantLibraryOptions> _options;

    internal ServiceBase(
        IEcosystem ecosystem,
        ILoggerService logger,
        IOptions<AssistantLibraryOptions> options
    ) {
        _environment = ecosystem.GetEnvironment();
        _logger = logger;
        _options = options;
    }
}