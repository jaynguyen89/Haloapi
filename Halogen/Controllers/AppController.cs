using Halogen.Parsers;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Halogen.Controllers; 

internal class AppController: ControllerBase {

    protected readonly ILoggerService _logger;
    protected readonly HalogenOptions _options;

    protected readonly string _environment;
    protected readonly bool _useLongerId;

    protected internal AppController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IOptions<HalogenOptions> options
    ) {
        _environment = ecosystem.GetEnvironment();
        _useLongerId = ecosystem.GetUseLongerId();
        
        _logger = logger;
        _options = options.Value;
    }
}