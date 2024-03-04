using HelperLibrary.Shared;

namespace Halogen.Bindings.ServiceBindings;

public sealed class HalogenConfigs {
    
    public string ClientBaseUri { get; set; } = null!;
    
    public int SaltMinLength { get; set; }
    
    public int SaltMaxLength { get; set; }
    
    public int TfaKeyMinLength { get; set; }
    
    public int TfaKeyMaxLength { get; set; }
    
    public int EmailTokenMinLength { get; set; }
    
    public int EmailTokenMaxLength { get; set; }
    
    public int EmailTokenValidityDuration { get; set; }
    
    public Enums.TimeUnit EmailTokenValidityDurationUnit { get; set; }
    
    public int OtpMinLength { get; set; }
    
    public int OtpMaxLength { get; set; }
    
    public int OtpValidityDuration { get; set; }
    
    public Enums.TimeUnit OtpValidityDurationUnit { get; set; }
    
    public int RecoveryTokenMinLength { get; set; }
    
    public int RecoveryTokenMaxLength { get; set; }
    
    public int RecoveryTokenValidityDuration { get; set; }
    
    public Enums.TimeUnit RecoveryTokenValidityDurationUnit { get; set; }
    
    public int PhoneTokenMinLength { get; set; }
    
    public int PhoneTokenMaxLength { get; set; }
    
    public int PhoneTokenValidityDuration { get; set; }
    
    public Enums.TimeUnit PhoneTokenValidityDurationUnit { get; set; }
    
    public int LoginFailedThreshold { get; set; }
    
    public int LockOutThreshold { get; set; }
    
    public int LockOutDuration { get; set; }
    
    public Enums.TimeUnit LockOutDurationUnit { get; set; }
    
    public string AccountActivationSmsContent { get; set; } = null!;
    
    public string AccountRecoverySmsContent { get; set; } = null!;
    
    public string TwoFactorPinSmsContent { get; set; } = null!;
    
    public string OneTimePasswordSmsContent { get; set; } = null!;
    
    public int AuthenticationValidityDuration { get; set; }
    
    public Enums.TimeUnit AuthenticationValidityDurationUnit { get; set; }
    
    public int SecretCodeValidityDuration { get; set; }
    
    public Enums.TimeUnit SecretCodeValidityDurationUnit { get; set; }

    public string SecretCodeSmsContent { get; set; } = null!;
    
    public bool EnableSecretCode { get; set; }
}