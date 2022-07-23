using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces.IServiceFactory;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Options;

namespace AssistantLibrary.Services.ServiceFactory;

public sealed class ClickatellSmsRestService: ServiceBase, ISmsService, IClickatellSmsRestService {
    
    public ClickatellSmsRestService(
        IEcosystem ecosystem,
        ILoggerService logger,
        IOptions<AssistantLibraryOptions> options
    ): base(ecosystem, logger, options) {
        
    }

    public async Task<string[]?> SendSingleSms(SingleSmsBinding binding) {
        throw new NotImplementedException();
    }

    public async Task<string[]?> SendMultipleSms(MultipleSmsBinding bindings) {
        throw new NotImplementedException();
    }
}
