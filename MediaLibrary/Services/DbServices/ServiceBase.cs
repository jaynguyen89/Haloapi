using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Configuration;

namespace MediaLibrary.Services.DbServices; 

internal class ServiceBase {

    protected readonly ILoggerService _logger;
    protected readonly IConfiguration _configuration;

    internal ServiceBase(
        ILoggerService logger,
        IConfiguration configuration
    ) {
        _logger = logger;
        _configuration = configuration;
    }
}