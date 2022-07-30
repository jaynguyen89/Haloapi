using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Configuration;

namespace AssistantLibrary.Services; 

public class ServiceBase {

    protected readonly string _environment;
    protected readonly ILoggerService _logger;
    protected readonly IConfiguration _configuration;

    protected readonly string _baseOptionKey;
    protected readonly string _clickatellBaseOptionKey;
    protected readonly string _twoFactorBaseOptionKey;
    protected readonly string _serviceFactoryBaseOptionKey;
    protected readonly string _mailServiceBaseOptionKey;
    protected readonly string _recaptchaBaseOptionKey;
    protected readonly string _qrGeneratorBaseOptionKey;

    internal ServiceBase(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ) {
        _environment = ecosystem.GetEnvironment();
        _logger = logger;
        _configuration = configuration;
        ParseBaseOptionKeys(
            out _baseOptionKey, out _clickatellBaseOptionKey, out _twoFactorBaseOptionKey, out _serviceFactoryBaseOptionKey,
            out _mailServiceBaseOptionKey, out _recaptchaBaseOptionKey, out _qrGeneratorBaseOptionKey
        );
    }

    private void ParseBaseOptionKeys(
        out string baseOptionKey, out string clickatellBaseOptionKey, out string twoFactorBaseOptionKey, out string serviceFactoryBaseOptionKey,
        out string mailServiceBaseOptionKey, out string recaptchaBaseOptionKey, out string qrGeneratorBaseOptionKey
    ) {
        baseOptionKey = $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}";
        (
            clickatellBaseOptionKey, twoFactorBaseOptionKey, serviceFactoryBaseOptionKey, mailServiceBaseOptionKey,
            recaptchaBaseOptionKey, qrGeneratorBaseOptionKey
        ) = _environment switch {
            Constants.Development => (
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Development.ClickatellHttpSettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Development.TwoFactorSettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Development.ServiceFactorySettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Development.MailServiceSettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Development.RecaptchaSettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Development.QrGeneratorSettings)}{Constants.Colon}"
            ),
            Constants.Staging => (
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Staging.ClickatellHttpSettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Staging.TwoFactorSettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Staging.ServiceFactorySettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Staging.MailServiceSettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Staging.RecaptchaSettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Staging.QrGeneratorSettings)}{Constants.Colon}"
            ),
            Constants.Production => (
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Production.ClickatellHttpSettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Production.TwoFactorSettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Production.ServiceFactorySettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Production.MailServiceSettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Production.RecaptchaSettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Production.QrGeneratorSettings)}{Constants.Colon}"
            ),
            _ => (
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Local.ClickatellHttpSettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Local.TwoFactorSettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Local.ServiceFactorySettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Local.MailServiceSettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Local.RecaptchaSettings)}{Constants.Colon}",
                $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Local.QrGeneratorSettings)}{Constants.Colon}"
            )
        };
    }
}