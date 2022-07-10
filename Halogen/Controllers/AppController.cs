using Microsoft.AspNetCore.Mvc;

namespace Halogen.Controllers; 

internal class AppController: ControllerBase {

    private readonly ILogger<AppController> _logger;

    protected internal AppController(ILogger<AppController> logger) {
        _logger = logger;
    }
}