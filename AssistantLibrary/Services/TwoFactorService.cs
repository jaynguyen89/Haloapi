using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using Google.Authenticator;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Configuration;

namespace AssistantLibrary.Services; 

public class TwoFactorService: ServiceBase, ITwoFactorService {

    private readonly TwoFactorAuthenticator _authenticator;

    public TwoFactorService() { }

    public TwoFactorService(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ): base(ecosystem, logger, configuration) {
        _authenticator = new TwoFactorAuthenticator();
    }
    
    public virtual TwoFactorData GetTwoFactorAuthenticationData(in GetTwoFactorBinding binding) {
        _logger.Log(new LoggerBinding<TwoFactorService> { Location = nameof(GetTwoFactorAuthenticationData) });
        
        var setupCodeData = _authenticator.GenerateSetupCode(
            _assistantConfigs.ProjectName,
            binding.EmailAddress,
            binding.SecretKey,
            true,
            _assistantConfigs.TwoFactorSettings.QrImageSize
        );

        return new TwoFactorData {
            QrCodeImageUrl = setupCodeData.QrCodeSetupImageUrl,
            ManualEntryKey = setupCodeData.ManualEntryKey,
        };
    }

    public virtual bool VerifyTwoFactorAuthenticationPin(in VerifyTwoFactorBinding binding) {
        _logger.Log(new LoggerBinding<TwoFactorService> { Location = nameof(VerifyTwoFactorAuthenticationPin) });
        
        return _authenticator.ValidateTwoFactorPIN(
            binding.SecretKey,
            binding.PinCode,
            TimeSpan.FromSeconds(_assistantConfigs.TwoFactorSettings.ToleranceDuration)
        );
    }
}