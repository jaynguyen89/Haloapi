using Halogen.Parsers;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;

namespace Halogen.Controllers; 

public class AppController: ControllerBase {

    protected readonly ILoggerService _logger;
    protected readonly IConfiguration _configuration;

    protected readonly string _environment;
    protected readonly bool _useLongerId;
    protected readonly string _smsContentsOptionKey;

    protected internal AppController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ) {
        _environment = ecosystem.GetEnvironment();
        _useLongerId = ecosystem.GetUseLongerId();
        
        _logger = logger;
        _configuration = configuration;
        _smsContentsOptionKey = _environment switch {
            Constants.Development => $"{nameof(HalogenOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(HalogenOptions.Development.SmsContents)}{Constants.Colon}",
            Constants.Staging => $"{nameof(HalogenOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(HalogenOptions.Staging.SmsContents)}{Constants.Colon}",
            Constants.Production => $"{nameof(HalogenOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(HalogenOptions.Production.SmsContents)}{Constants.Colon}",
            _ => $"{nameof(HalogenOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(HalogenOptions.Local.SmsContents)}{Constants.Colon}"
        };
    }
}