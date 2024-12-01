using Halogen.Bindings;
using Halogen.Bindings.ApiBindings;
using Halogen.Bindings.ServiceBindings;
using Halogen.DbModels;
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

    protected readonly string _environment;
    protected readonly bool _useLongerId;
    protected readonly HalogenConfigs _haloConfigs;

    public AppController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        HalogenConfigs configs
    ) {
        _environment = ecosystem.GetEnvironment();
        _useLongerId = ecosystem.GetUseLongerId();
        
        _logger = logger;
        _configuration = configuration;
        _haloConfigs = configs;
        
        var (isEssential, maxAge, expiration) = (
            bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings.IsEssential)}")!),
            int.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings.MaxAge)}")!),
            int.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings.Expiration)}")!)
        );

        _cookieOptions = new CookieOptions {
            Domain = "http://localhost:3000",
            Path = Constants.FSlash,
            MaxAge = TimeSpan.FromDays(maxAge),
            IsEssential = isEssential,
            HttpOnly = true,
            Expires = DateTimeOffset.FromUnixTimeMilliseconds(expiration.ToMilliseconds(Enums.TimeUnit.Day)),
            SameSite = SameSiteMode.None,
            Secure = isEssential,
        };
    }

    protected bool? IsSecretCodeValid(string secretCode, Account account) {
        if (!Equals(account.SecretCode, secretCode)) return default;
        return !account.SecretCodeTimestamp.HasValue ||
               account.SecretCodeTimestamp.Value.Compute(_haloConfigs.SecretCodeValidityDuration, _haloConfigs.SecretCodeValidityDurationUnit) >= DateTime.UtcNow;
    }

    protected bool IsDeviceTrusted(DeviceInformation device, TrustedDevice[] trustedDevices) {
        //Todo: check if the device is of trusted device
        return true;
    }
}