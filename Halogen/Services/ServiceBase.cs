using HelperLibrary.Shared.Logger;

namespace Halogen.Services; 

public class ServiceBase: IServiceBase {
    
    protected readonly ILoggerService _logger;
    protected readonly HttpContext? _httpContext;

    protected internal ServiceBase() { }

    protected internal ServiceBase(
        ILoggerService logger
    ) {
        _logger = logger;
    }
    
    protected internal ServiceBase(
        ILoggerService logger,
        HttpContext httpContext
    ) {
        _logger = logger;
        _httpContext = httpContext;
    }
}