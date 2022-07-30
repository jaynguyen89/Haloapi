using Halogen.Services.AppServices.Interfaces;
using HelperLibrary.Shared.Logger;

namespace Halogen.Services.AppServices.Services;

public class AppServiceBase: IAppServiceBase {

    protected readonly ILoggerService _logger;

    protected internal AppServiceBase(
        ILoggerService logger
    ) {
        _logger = logger;
    }
}