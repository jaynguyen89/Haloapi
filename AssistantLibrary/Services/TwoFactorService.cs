using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using Google.Authenticator;
using HelperLibrary;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Configuration;

namespace AssistantLibrary.Services; 

public sealed class TwoFactorService: ServiceBase, ITwoFactorService {

    private readonly TwoFactorAuthenticator _authenticator;

    public TwoFactorService(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ): base(ecosystem, logger, configuration) {
        _authenticator = new TwoFactorAuthenticator();
    }
    
    public TwoFactorData GetTwoFactorAuthenticationData(in GetTwoFactorBinding binding) {
        _logger.Log(new LoggerBinding<TwoFactorService> { Location = nameof(GetTwoFactorAuthenticationData) });
        
        var (projectName, qrImageSize) = (
            _configuration.AsEnumerable().Single(x => x.Key.Equals($"{nameof(AssistantLibraryOptions)}{Constants.Colon}{nameof(AssistantLibraryOptions.Local.ProjectName)}")).Value,
            _configuration.AsEnumerable().Single(x => x.Key.Equals($"{_twoFactorBaseOptionKey}{nameof(AssistantLibraryOptions.Local.TwoFactorSettings.QrImageSize)}")).Value
        );
        
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
        var tolerance = _configuration.AsEnumerable().Single(x => x.Key.Equals($"{_twoFactorBaseOptionKey}{nameof(AssistantLibraryOptions.Local.TwoFactorSettings.ToleranceDuration)}")).Value;
        
        return _authenticator.ValidateTwoFactorPIN(
            binding.SecretKey,
            binding.PinCode,
            TimeSpan.FromSeconds(tolerance.ToInt() ?? Constants.TwoFactorDefaultTolerance)
        );
    }
}