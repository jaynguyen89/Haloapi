using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Configuration;
using QRCoder;

namespace AssistantLibrary.Services;

public sealed class AssistantConfigProvider: IAssistantConfigProvider {
    
    private readonly IEcosystem _ecosystem;
    private readonly ILoggerService _logger;
    private readonly IConfiguration _configuration;

    public AssistantConfigProvider(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ) {
        _ecosystem = ecosystem;
        _logger = logger;
        _configuration = configuration;
    }

    public AssistantConfigs GetAssistantConfigs() {
        _logger.Log(new LoggerBinding<AssistantConfigProvider> { Location = nameof(GetAssistantConfigs) });
        
        var environment = _ecosystem.GetEnvironment();
        var (
            baseOptionKey, clickatellBaseOptionKey, twoFactorBaseOptionKey, serviceFactoryBaseOptionKey,
            mailServiceBaseOptionKey, recaptchaBaseOptionKey, qrGeneratorBaseOptionKey
        ) = (
            $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{environment}{Constants.Colon}",
            $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Local.ClickatellHttpSettings)}{Constants.Colon}",
            $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Local.TwoFactorSettings)}{Constants.Colon}",
            $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Local.ServiceFactorySettings)}{Constants.Colon}",
            $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Local.MailServiceSettings)}{Constants.Colon}",
            $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Local.RecaptchaSettings)}{Constants.Colon}",
            $"{nameof(AssistantLibraryOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(AssistantLibraryOptions.Local.QrGeneratorSettings)}{Constants.Colon}"
        );

        return new AssistantConfigs {
            ProjectName = ReadConfigValue($"{baseOptionKey}{nameof(AssistantLibraryOptions.Local.ProjectName)}") ?? Constants.ProjectName,
            RsaKeyLength = int.Parse(ReadConfigValue($"{baseOptionKey}{nameof(AssistantLibraryOptions.Local.RsaKeyLength)}")!),
            TwoFactorSettings = new Bindings.TwoFactorSettings {
                QrImageSize = int.Parse(ReadConfigValue($"{twoFactorBaseOptionKey}{nameof(AssistantLibraryOptions.Local.TwoFactorSettings.QrImageSize)}")!),
                ToleranceDuration = int.Parse(ReadConfigValue($"{twoFactorBaseOptionKey}{nameof(AssistantLibraryOptions.Local.TwoFactorSettings.ToleranceDuration)}")!),
            },
            RecaptchaSettings = new Bindings.RecaptchaSettings {
                SecretKey = ReadConfigValue($"{recaptchaBaseOptionKey}{nameof(AssistantLibraryOptions.Local.RecaptchaSettings.SecretKey)}")!,
                Endpoint = ReadConfigValue($"{recaptchaBaseOptionKey}{nameof(AssistantLibraryOptions.Local.RecaptchaSettings.Endpoint)}")!,
                RequestContentType = ReadConfigValue($"{recaptchaBaseOptionKey}{nameof(AssistantLibraryOptions.Local.RecaptchaSettings.RequestContentType)}")!,
            },
            MailServiceSettings = new Bindings.MailServiceSettings {
                MailServerHost = ReadConfigValue($"{mailServiceBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.MailServerHost)}")!,
                MailServerPort = int.Parse(ReadConfigValue($"{mailServiceBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.MailServerPort)}")!),
                UseSsl = bool.Parse(ReadConfigValue($"{mailServiceBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.UseSsl)}")!),
                Timeout = int.Parse(ReadConfigValue($"{mailServiceBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.Timeout)}") ?? $"{Constants.SecondsPerMinute / 2}"),
                EmailAddressCredential = ReadConfigValue($"{mailServiceBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.ServerCredentials)}{Constants.Colon}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.ServerCredentials.EmailAddress)}")!,
                PasswordCredential = ReadConfigValue($"{mailServiceBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.ServerCredentials)}{Constants.Colon}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.ServerCredentials.Password)}")!,
                DefaultSenderEmailAddress = ReadConfigValue($"{mailServiceBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.DefaultSenderAddress)}")!,
                DefaultSenderName = ReadConfigValue($"{mailServiceBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.DefaultSenderName)}")!,
                PlaceholderHalogenLogoUrl = ReadConfigValue($"{mailServiceBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.DefaultPlaceholders)}{Constants.Colon}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.DefaultPlaceholders.HalogenLogoUrl)}")!,
                PlaceholderClientBaseUri = ReadConfigValue($"{mailServiceBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.DefaultPlaceholders)}{Constants.Colon}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.DefaultPlaceholders.ClientBaseUri)}")!,
                PlaceholderClientAppName = ReadConfigValue($"{mailServiceBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.DefaultPlaceholders)}{Constants.Colon}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.DefaultPlaceholders.ClientApplicationName)}")!,
            },
            ServiceFactorySettings = new Bindings.ServiceFactorySettings {
                ActiveMailService = ReadConfigValue($"{serviceFactoryBaseOptionKey}{nameof(AssistantLibraryOptions.Local.ServiceFactorySettings.ActiveMailService)}")!,
                ActiveSmsService = ReadConfigValue($"{serviceFactoryBaseOptionKey}{nameof(AssistantLibraryOptions.Local.ServiceFactorySettings.ActiveSmsService)}")!,
                ActiveTfaService = ReadConfigValue($"{serviceFactoryBaseOptionKey}{nameof(AssistantLibraryOptions.Local.ServiceFactorySettings.ActiveTfaService)}")!,
            },
            ClickatellSettings = new ClickatellSettings {
                ApiKey = ReadConfigValue($"{clickatellBaseOptionKey}{nameof(AssistantLibraryOptions.Local.ClickatellHttpSettings.ApiKey)}")!,
                HttpEndpoint = ReadConfigValue($"{clickatellBaseOptionKey}{nameof(AssistantLibraryOptions.Local.ClickatellHttpSettings.HttpEndpoint)}")!,
                PhoneNumber = ReadConfigValue($"{clickatellBaseOptionKey}{nameof(AssistantLibraryOptions.Local.ClickatellHttpSettings.DevTestPhoneNumber)}")!,
                RequestContentType = ReadConfigValue($"{clickatellBaseOptionKey}{nameof(AssistantLibraryOptions.Local.ClickatellHttpSettings.RequestContentType)}")!,
            },
            QrGeneratorSettings = new Bindings.QrGeneratorSettings {
                ImageSize = int.Parse(ReadConfigValue($"{qrGeneratorBaseOptionKey}{nameof(AssistantLibraryOptions.Local.QrGeneratorSettings.ImageSize)}")!),
                DarkColor = ReadConfigValue($"{qrGeneratorBaseOptionKey}{nameof(AssistantLibraryOptions.Local.QrGeneratorSettings.DarkColor)}")!,
                LightColor = ReadConfigValue($"{qrGeneratorBaseOptionKey}{nameof(AssistantLibraryOptions.Local.QrGeneratorSettings.LightColor)}")!,
                EccLevel = Enum.Parse<QRCodeGenerator.ECCLevel>(ReadConfigValue($"{qrGeneratorBaseOptionKey}{nameof(AssistantLibraryOptions.Local.QrGeneratorSettings.EccLevel)}")!),
                WithLogo = bool.Parse(ReadConfigValue($"{qrGeneratorBaseOptionKey}{nameof(AssistantLibraryOptions.Local.QrGeneratorSettings.WithLogo)}")!),
                LogoName = ReadConfigValue($"{qrGeneratorBaseOptionKey}{nameof(AssistantLibraryOptions.Local.QrGeneratorSettings.LogoName)}")!,
            }
        };
    }

    private string? ReadConfigValue(string key) => _configuration.AsEnumerable().Single(x => x.Key.Equals(key)).Value;
}