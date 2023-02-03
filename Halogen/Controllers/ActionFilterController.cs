using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;

namespace Halogen.Controllers;

public sealed class ActionFilterController: AppController {

    public ActionFilterController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ): base(ecosystem, logger, configuration) {
        
    }

    public JsonResult SecurityResult() {
        return null;
    }

    public JsonResult ServiceRateResult() {
        return null;
    }
}