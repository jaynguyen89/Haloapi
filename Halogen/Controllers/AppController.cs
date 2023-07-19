using Halogen.Bindings;
using Halogen.Bindings.ApiBindings;
using Halogen.DbModels;
using HelperLibrary;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;

namespace Halogen.Controllers; 

public class AppController: ControllerBase {

    protected readonly ILoggerService _logger;
    protected readonly IConfiguration _configuration;
    protected readonly CookieOptions _cookieOptions;
    protected readonly string _clientApplicationName;

    protected readonly string _environment;
    protected readonly bool _useLongerId;
    protected readonly string _baseSessionSettingsOptionKey;
    protected readonly string _baseSecuritySettingsOptionKey;
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

        _clientApplicationName = configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(HalogenOptions.Development.ClientApplicationName)}");
        (_baseSessionSettingsOptionKey, _baseSecuritySettingsOptionKey, _smsContentsOptionKey) = (
            $"{nameof(HalogenOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings)}{Constants.Colon}",
            $"{nameof(HalogenOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(HalogenOptions.Local.SecuritySettings)}{Constants.Colon}",
            $"{nameof(HalogenOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(HalogenOptions.Local.SmsContents)}{Constants.Colon}"
        );
        
        var (isEssential, maxAge, expiration) = (
            bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings.IsEssential)}")),
            int.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings.MaxAge)}")),
            int.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings.Expiration)}"))
        );

        _cookieOptions = new CookieOptions {
            Domain = nameof(Halogen),
            Path = Constants.FSlash,
            MaxAge = TimeSpan.FromDays(maxAge),
            IsEssential = isEssential,
            HttpOnly = true,
            Expires = DateTimeOffset.FromUnixTimeMilliseconds(expiration.ToMilliseconds(Enums.TimeUnit.Day)),
            SameSite = SameSiteMode.None,
            Secure = isEssential,
        };
    }

    protected bool IsDeviceTrusted(DeviceInformation device, TrustedDevice[] trustedDevices) {
        //Todo: check if the device is of trusted device
        return true;
    }
}