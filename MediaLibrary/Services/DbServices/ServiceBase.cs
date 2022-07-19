using HelperLibrary.Shared.Logger;

namespace MediaLibrary.Services.DbServices; 

internal class ServiceBase {

    protected readonly ILoggerService _logger;
    protected readonly MediaLibraryOptions _option;

    internal ServiceBase(
        ILoggerService logger,
        MediaLibraryOptions options
    ) {
        _logger = logger;
        _option = options;
    }
}