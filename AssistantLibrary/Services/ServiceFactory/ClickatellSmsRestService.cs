using AssistantLibrary.Interfaces.IServiceFactory;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Options;

namespace AssistantLibrary.Services.ServiceFactory;

public sealed class GlobalSmsService: ServiceBase, ISmsService, IGlobalSmsService {
    
    public GlobalSmsService(
        IEcosystem ecosystem,
        ILoggerService logger,
        IOptions<AssistantLibraryOptions> options
    ): base(ecosystem, logger, options) {
        
    }
}
