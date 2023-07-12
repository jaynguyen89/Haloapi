using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Halogen.Controllers; 

[ApiController]
[Route("settings")]
public sealed class SettingsController {

    [HttpGet("api-settings/{environment}")]
    public IActionResult? GetApiSettings([FromRoute] string environment) {
        return null;
    }
}