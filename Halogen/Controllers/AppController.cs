using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Interfaces.IServiceFactory;
using Halogen.Bindings;
using Halogen.Bindings.ApiBindings;
using Halogen.Bindings.ServiceBindings;
using Halogen.Bindings.ViewModels;
using Halogen.DbModels;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Preference = Halogen.Bindings.ServiceBindings.Preference;

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
        _logger.Log(new LoggerBinding<AppController> { Location = nameof(IsSecretCodeValid) });
        if (!Equals(account.SecretCode, secretCode)) return default;
        return !account.SecretCodeTimestamp.HasValue ||
               account.SecretCodeTimestamp.Value.Compute(_haloConfigs.SecretCodeValidityDuration, _haloConfigs.SecretCodeValidityDurationUnit) >= DateTime.UtcNow;
    }

    protected bool IsDeviceTrusted(DeviceInformation device, TrustedDevice[] trustedDevices) {
        _logger.Log(new LoggerBinding<AppController> { Location = nameof(IsDeviceTrusted) });
        //Todo: check if the device is among the trusted devices
        return true;
    }

    protected async Task<bool?> SendNotification(NotificationContent content, IMailService mailService, ISmsService smsService) {
        _logger.Log(new LoggerBinding<AppController> { Location = nameof(SendNotification) });

        var preference = JsonConvert.DeserializeObject<Preference>(HttpContext.Session.GetString(Enums.SessionKey.Preference.GetValue()!) ?? string.Empty);
        if (preference is null) return default;
        
        var authenticatedUser = JsonConvert.DeserializeObject<AuthenticatedUser>(HttpContext.Session.GetString(Enums.SessionKey.AuthenticatedUser.GetValue()!) ?? string.Empty);
        if (authenticatedUser is null) return default;

        content.Placeholders ??= new Dictionary<string, string>();
        content.Placeholders.Add("USERNAME", authenticatedUser.FullName ?? authenticatedUser.Username);
        
        if (preference.Preferences.SecurityPreference.PrioritizeLoginNotificationsOverEmail) {
            var mailBinding = new MailBinding {
                ToReceivers = [new Recipient { EmailAddress = authenticatedUser.EmailAddress! }],
                Title = content.Title,
                TemplateName = content.MailTemplateName,
                Placeholders = content.Placeholders,
            };
            
            return await mailService.SendSingleEmail(mailBinding);
        }
        else {
            var smsContent = content.SmsContent!;
            foreach (var (placeholder, value) in content.Placeholders)
                smsContent = smsContent.Replace(placeholder, value);

            var smsBinding = new SingleSmsBinding {
                SmsContent = smsContent,
                Receivers = [authenticatedUser.PhoneNumber!.ToPhoneNumber()],
            };
            
            var result = await smsService.SendSingleSms(smsBinding);
            if (result is null) return default;
            return result.Length == 0;
        }
    }
}