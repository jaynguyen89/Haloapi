using Halogen.Services.HostedServices.Interfaces;
using HelperLibrary.Shared.Logger;

namespace Halogen.Services.HostedServices; 

public class HostedServiceBase: ServiceBase, IHostedServiceBase {
    
    protected internal HostedServiceBase(ILoggerService logger) : base(logger) { }
    
    public Task StartAsync(CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }
}