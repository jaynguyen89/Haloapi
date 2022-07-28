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
            Constants.Development => (_options.Dev.ProjectName, _options.Dev.TwoFactorSettings.QrImageSize),
            Constants.Staging => (_options.Stg.ProjectName, _options.Stg.TwoFactorSettings.QrImageSize),
            Constants.Production => (_options.Prod.ProjectName, _options.Prod.TwoFactorSettings.QrImageSize),
            _ => (_options.Loc.ProjectName, _options.Loc.TwoFactorSettings.QrImageSize)
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
            Constants.Development => _options.Dev.TwoFactorSettings.ToleranceDuration,
            Constants.Staging => _options.Stg.TwoFactorSettings.ToleranceDuration,
            Constants.Production => _options.Prod.TwoFactorSettings.ToleranceDuration,
            _ => _options.Loc.TwoFactorSettings.ToleranceDuration
        };
        
        return _authenticator.ValidateTwoFactorPIN(
            binding.SecretKey,
            binding.PinCode,
            TimeSpan.FromSeconds(tolerance.ToInt() ?? Constants.TwoFactorDefaultTolerance)
        );
    }
}