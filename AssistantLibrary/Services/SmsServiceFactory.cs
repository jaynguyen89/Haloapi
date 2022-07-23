using AssistantLibrary.Interfaces;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Options;

namespace AssistantLibrary.Services; 

public sealed class SmsService: ServiceBase, ISmsService {

    public SmsService(
        IEcosystem ecosystem,
        ILoggerService logger,
        IOptions<AssistantLibraryOptions> options
    ): base(ecosystem, logger, options) {
        
    }
}