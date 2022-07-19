using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using Google.Authenticator;
using HelperLibrary;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Options;

namespace AssistantLibrary.Services; 

public sealed class TwoFactorService: ServiceBase, ITwoFactorService {

    private readonly TwoFactorAuthenticator _authenticator;

    public TwoFactorService(
        IEcosystem ecosystem,
        ILoggerService logger,
        IOptions<AssistantLibraryOptions> options
    ): base(ecosystem, logger, options) {
        _authenticator = new TwoFactorAuthenticator();
    }
    
    public TwoFactorData GetTwoFactorAuthenticationData(in GetTwoFactorBinding binding) {
        _logger.Log(new LoggerBinding<TwoFactorService> { Location = nameof(GetTwoFactorAuthenticationData) });
        
        var (projectName, qrImageSize) = _environment switch {
            Constants.Development => (_options.Value.Dev.ProjectName, _options.Value.Dev.TwoFactorSettings.QrImageSize),
            Constants.Staging => (_options.Value.Stg.ProjectName, _options.Value.Stg.TwoFactorSettings.QrImageSize),
            _ => (_options.Value.Prod.ProjectName, _options.Value.Prod.TwoFactorSettings.QrImageSize ?? string.Empty)
        };
        
        var setupCodeData = _authenticator.GenerateSetupCode(
            projectName ?? binding.ProjectName,
            binding.EmailAddress,
            binding.SecretKey,
            true,
            qrImageSize.ToInt() ?? binding.ImageSize
        );

        return new TwoFactorData {
            QrCodeImageUrl = setupCodeData.QrCodeSetupImageUrl,
            ManualEntryKey = setupCodeData.ManualEntryKey
        };
    }

    public bool VerifyTwoFactorAuthenticationPin(in VerifyTwoFactorBinding binding) {
        _logger.Log(new LoggerBinding<TwoFactorService> { Location = nameof(VerifyTwoFactorAuthenticationPin) });

        var tolerance = _environment switch {
            Constants.Development => _options.Value.Dev.TwoFactorSettings.ToleranceDuration,
            Constants.Staging => _options.Value.Stg.TwoFactorSettings.ToleranceDuration,
            _ => _options.Value.Prod.TwoFactorSettings.ToleranceDuration
        };
        
        return _authenticator.ValidateTwoFactorPIN(
            binding.SecretKey,
            binding.PinCode,
            TimeSpan.FromSeconds(tolerance.ToInt() ?? Constants.TwoFactorDefaultTolerance)
        );
    }
}