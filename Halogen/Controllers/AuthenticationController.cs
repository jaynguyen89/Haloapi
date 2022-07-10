namespace Halogen.Controllers; 

internal sealed class AuthenticationController: AppController {

    internal AuthenticationController(
        ILogger<AuthenticationController> logger
    ) : base(logger) {
        
    }
}