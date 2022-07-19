using HelperLibrary.Shared.Logger;

namespace AmazonLibrary.Services; 

internal class ServiceBase {
    
    protected readonly ILoggerService _logger;
    protected readonly AmazonLibraryOptions _options;
    
    internal ServiceBase(
        ILoggerService logger,
        AmazonLibraryOptions options
    ) {
        _logger = logger;
        _options = options;
    }
}