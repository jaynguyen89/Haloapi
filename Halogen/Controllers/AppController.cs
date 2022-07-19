using Halogen.Parsers;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;

namespace Halogen.Controllers; 

internal class AppController: ControllerBase {

    protected readonly ILoggerService _logger;
    protected readonly HalogenOptions _options;

    protected internal AppController(
        ILoggerService logger,
        HalogenOptions options
    ) {
        _logger = logger;
        _options = options;
    }
}