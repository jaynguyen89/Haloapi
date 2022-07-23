using Halogen.Parsers;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;

namespace Halogen.Controllers; 

internal class AppController: ControllerBase {

    protected readonly ILoggerService _logger;
    protected readonly HalogenOptions _options;

    protected readonly string _environment;

    protected internal AppController(
        IEcosystem ecosystem,
        ILoggerService logger,
        HalogenOptions options
    ) {
        _environment = ecosystem.GetEnvironment();
        _logger = logger;
        _options = options;
    }
}