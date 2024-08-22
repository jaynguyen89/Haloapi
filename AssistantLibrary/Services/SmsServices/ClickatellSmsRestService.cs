using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces.SmsServices;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Configuration;

namespace AssistantLibrary.Services.SmsServices;

public sealed class ClickatellSmsRestService: ServiceBase, IClickatellSmsRestService {
    
    public ClickatellSmsRestService(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ): base(ecosystem, logger, configuration) {
        
    }

    public async Task<string[]?> SendSingleSms(SingleSmsBinding binding) {
        throw new NotImplementedException();
    }

    public async Task<string[]?> SendMultipleSms(MultipleSmsBinding bindings) {
        throw new NotImplementedException();
    }
}
