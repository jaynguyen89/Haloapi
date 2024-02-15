﻿using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;

namespace Halogen.Auxiliaries;

public sealed class HaloConfigProvider: IHaloConfigProvider {

    private readonly IEcosystem _ecosystem;
    private readonly ILoggerService _logger;
    private readonly IConfiguration _configuration;

    public HaloConfigProvider(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ) {
        _ecosystem = ecosystem;
        _logger = logger;
        _configuration = configuration;
    }

    public HalogenConfigs GetHalogenConfigs() {
        _logger.Log(new LoggerBinding<HaloConfigProvider> { Location = nameof(GetHalogenConfigs) });

        var environment = _ecosystem.GetEnvironment();
        
        var (baseSessionSettingsOptionKey, baseSecuritySettingsOptionKey, smsContentsOptionKey) = (
            $"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings)}{Constants.Colon}",
            $"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.SecuritySettings)}{Constants.Colon}",
            $"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.SmsContents)}{Constants.Colon}"
        );
        
        var (
            saltMinLength, saltMaxLength, tfaKeyMinLength, tfaKeyMaxLength,
            emailTokenMinLength, emailTokenMaxLength, emailTokenValidityDuration, emailTokenValidityDurationUnit
        ) = (
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.SaltMinLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.SaltMaxLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.TfaKeyMinLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.TfaKeyMaxLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.EmailTokenMinLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.EmailTokenMaxLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.EmailTokenValidityDuration)}")),
            _configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.EmailTokenValidityDurationUnit)}").ToEnum(Enums.TimeUnit.Hour)
        );
        
        var (
            otpMinLength, otpMaxLength, otpValidityDuration, otpValidityDurationUnit,
            recoveryTokenMinLength, recoveryTokenMaxLength, recoveryTokenValidityDuration, recoveryTokenValidityDurationUnit
        ) = (
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.OtpMinLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.OtpMaxLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.OtpValidityDuration)}")),
            _configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.OtpValidityDurationUnit)}").ToEnum(Enums.TimeUnit.Hour),
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.RecoveryTokenMinLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.RecoveryTokenMaxLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.RecoveryTokenValidityDuration)}")),
            _configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.RecoveryTokenValidityDurationUnit)}").ToEnum(Enums.TimeUnit.Hour)
        );
        
        var (
            phoneTokenMinLength, phoneTokenMaxLength, phoneTokenValidityDuration, phoneTokenValidityDurationUnit,
            loginFailedThreshold, lockOutThreshold, lockOutDuration, lockOutDurationUnit
        ) = (
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.PhoneTokenMinLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.PhoneTokenMaxLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.PhoneTokenValidityDuration)}")),
            _configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.PhoneTokenValidityDurationUnit)}").ToEnum(Enums.TimeUnit.Hour),
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.LoginFailedThreshold)}")),
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.LockOutThreshold)}")),
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.LockOutDuration)}")),
            _configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.LockOutDurationUnit)}").ToEnum(Enums.TimeUnit.Hour)
        );
        
        var (
            accountActivationSmsContent, accountRecoverySmsContent, twoFactorPinSmsContent, oneTimePasswordSmsContent,
            authenticationValidityDuration, authenticationValidityDurationUnit, secretCodeValidityDuration, secretCodeValidityDurationUnit
        ) = (
            _configuration.GetValue<string>($"{smsContentsOptionKey}{nameof(HalogenOptions.Local.SmsContents.AccountActivationSms)}"),
            _configuration.GetValue<string>($"{smsContentsOptionKey}{nameof(HalogenOptions.Local.SmsContents.AccountRecoverySms)}"),
            _configuration.GetValue<string>($"{smsContentsOptionKey}{nameof(HalogenOptions.Local.SmsContents.TwoFactorPinSms)}"),
            _configuration.GetValue<string>($"{smsContentsOptionKey}{nameof(HalogenOptions.Local.SmsContents.OneTimePasswordSms)}"),
            int.Parse(_configuration.GetValue<string>($"{baseSessionSettingsOptionKey}{nameof(HalogenOptions.Local.SessionSettings.AuthenticationValidityDuration)}")),
            _configuration.GetValue<string>($"{baseSessionSettingsOptionKey}{nameof(HalogenOptions.Local.SessionSettings.AuthenticationValidityDurationUnit)}").ToEnum(Enums.TimeUnit.Minute),
            int.Parse(_configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.SecretCodeValidityDuration)}")),
            _configuration.GetValue<string>($"{baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.SecretCodeValidityDurationUnit)}").ToEnum(Enums.TimeUnit.Minute)
        );

        var secretCodeSmsContent = _configuration.GetValue<string>($"{smsContentsOptionKey}{nameof(HalogenOptions.Local.SmsContents.SecretCodeSms)}");

        return new HalogenConfigs {
            ClientBaseUri = _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.ClientBaseUri)}"),
            SaltMinLength = saltMinLength,
            SaltMaxLength = saltMaxLength,
            TfaKeyMinLength = tfaKeyMinLength,
            TfaKeyMaxLength = tfaKeyMaxLength,
            EmailTokenMinLength = emailTokenMinLength,
            EmailTokenMaxLength = emailTokenMaxLength,
            EmailTokenValidityDuration = emailTokenValidityDuration,
            EmailTokenValidityDurationUnit = emailTokenValidityDurationUnit,
            OtpMinLength = otpMinLength,
            OtpMaxLength = otpMaxLength,
            OtpValidityDuration = otpValidityDuration,
            OtpValidityDurationUnit = otpValidityDurationUnit,
            RecoveryTokenMinLength = recoveryTokenMinLength,
            RecoveryTokenMaxLength = recoveryTokenMaxLength,
            RecoveryTokenValidityDuration = recoveryTokenValidityDuration,
            RecoveryTokenValidityDurationUnit = recoveryTokenValidityDurationUnit,
            PhoneTokenMinLength = phoneTokenMinLength,
            PhoneTokenMaxLength = phoneTokenMaxLength,
            PhoneTokenValidityDuration = phoneTokenValidityDuration,
            PhoneTokenValidityDurationUnit = phoneTokenValidityDurationUnit,
            LoginFailedThreshold = loginFailedThreshold,
            LockOutThreshold = lockOutThreshold,
            LockOutDuration = lockOutDuration,
            LockOutDurationUnit = lockOutDurationUnit,
            AccountActivationSmsContent = accountActivationSmsContent,
            AccountRecoverySmsContent = accountRecoverySmsContent,
            TwoFactorPinSmsContent = twoFactorPinSmsContent,
            OneTimePasswordSmsContent = oneTimePasswordSmsContent,
            AuthenticationValidityDuration = authenticationValidityDuration,
            AuthenticationValidityDurationUnit = authenticationValidityDurationUnit,
            SecretCodeValidityDuration = secretCodeValidityDuration,
            SecretCodeValidityDurationUnit = secretCodeValidityDurationUnit,
            SecretCodeSmsContent = secretCodeSmsContent,
        };
    }
}