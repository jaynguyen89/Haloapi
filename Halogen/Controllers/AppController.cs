using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;

namespace Halogen.Controllers; 

public class AppController: ControllerBase {

    protected readonly ILoggerService _logger;
    protected readonly IConfiguration _configuration;

    protected readonly string _environment;
    protected readonly bool _useLongerId;

    protected internal AppController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ) {
        _environment = ecosystem.GetEnvironment();
        _useLongerId = ecosystem.GetUseLongerId();
        
        _logger = logger;
        _configuration = configuration;
    }
}