using HelperLibrary.Shared.Logger;

namespace Halogen.Services; 

public class ServiceBase: IServiceBase {
    
    protected readonly ILoggerService _logger;

    protected internal ServiceBase(
        ILoggerService logger
    ) {
        _logger = logger;
    }
}