using Halogen.Services.AppServices.Interfaces;
using HelperLibrary.Shared.Logger;

namespace Halogen.Services.AppServices.Services;

public class AppServiceBase: ServiceBase, IAppServiceBase {

    protected internal AppServiceBase(
        ILoggerService logger
    ): base(logger) {
    }
}