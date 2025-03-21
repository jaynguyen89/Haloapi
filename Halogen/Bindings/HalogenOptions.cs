﻿#pragma warning disable 8618
namespace Halogen.Bindings;
    
public sealed class HalogenOptions {

    public class CacheKeys {
        public string TelephoneCodes { get; set; }
    }

    public sealed class ServerSettings {
        public string AwsAccessKeyId { get; set; }
        public string AwsSecretAccessKey { get; set; }
        public string AwsRegion { get; set; }
        public string AwsLogGroupName { get; set; }
        public string EnableSecretCode { get; set; }
    }

    public class Local {
        public string CsrfHeaderName { get; set; }
        public string CookieShouldCheckConsent { get; set; }
        public string ClientBaseUri { get; set; }
        
        public DbSettings DbSettings { get; set; }
        public ServiceSettings ServiceSettings { get; set; }
        public SecuritySettings SecuritySettings { get; set; }
        public SmsContents SmsContents { get; set; }

        public sealed class SwaggerInfo {
            public string Version { get; set; }
            public string ApiName { get; set; }
            public string Description { get; set; }
        }

        public sealed class JwtSettings {
            public string RequireHttpsMetadata { get; set; }
            public string SaveToken { get; set; }
            
            public sealed class TokenValidationParameters {
                public string ValidateIssuers { get; set; }
                public string ValidateIssuerSigningKey { get; set; }
                public string ValidateAudience { get; set; }
                public string ValidateLifetime { get; set; }
                public string RequireExpirationTime { get; set; }
                public string IgnoreTrailingSlashWhenValidatingAudience { get; set; }
                public string ClockSkew { get; set; }
                public string ValidIssuers { get; set; }
                public string ValidAudiences { get; set; }
                public string IssuerSigningKey { get; set; }
                public string Expiration { get; set; }
            }
        }
        
        public sealed class SessionSettings {
            public string Name { get; set; }
            public string IdleTimeout { get; set; }
            public string IsEssential { get; set; }
            public string MaxAge { get; set; }
            public string Expiration { get; set; }
            public string CorsOrigins { get; set; }
            public string AuthenticationValidityDuration { get; set; }
            public string AuthenticationValidityDurationUnit { get; set; }
        }

        public sealed class MemoryCacheSettings {
            public string IsEnabled { get; set; }
            public string Size { get; set; }
            public string Compaction { get; set; }
            public string ScanFrequency { get; set; }
            public string SlidingExpiration { get; set; }
        }

        public sealed class RedisCacheSettings {
            public string IsEnabled { get; set; }
            public string SlidingExpiration { get; set; }
            public string AbsoluteExpiration { get; set; }
            
            public sealed class Connection {
                public string Endpoint { get; set; }
                public string Port { get; set; }
                public string Password { get; set; }
                public string Ssl { get; set; }
                public string DefaultDb { get; set; }
                public string AbortConnect { get; set; }
                public string AllowAdmin { get; set; }
                public string InstanceName { get; set; }
            }
        }
    }

    public sealed class Development: Local { }
    
    public sealed class Staging: Local { }

    public sealed class Production: Local { }
}

public sealed class DbSettings {
    public string WinEndpoint { get; set; }
    public string LinEndpoint { get; set; }
    public string Port { get; set; }
    public string DbName { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public sealed class ServiceSettings {
    public string TwoFactorEnabled { get; set; }
    public string RecaptchaEnabled { get; set; }
    public string SupportedSocialAccountForRegistration { get; set; }
}

public sealed class SecuritySettings {
    public string SaltMinLength { get; set; }
    public string SaltMaxLength { get; set; }
    public string TfaKeyMinLength { get; set; }
    public string TfaKeyMaxLength { get; set; }
    public string EmailTokenMinLength { get; set; }
    public string EmailTokenMaxLength { get; set; }
    public string EmailTokenValidityDuration { get; set; }
    public string EmailTokenValidityDurationUnit { get; set; }
    public string OtpMinLength { get; set; }
    public string OtpMaxLength { get; set; }
    public string OtpValidityDuration { get; set; }
    public string OtpValidityDurationUnit { get; set; }
    public string RecoveryTokenMinLength { get; set; }
    public string RecoveryTokenMaxLength { get; set; }
    public string RecoveryTokenValidityDuration { get; set; }
    public string RecoveryTokenValidityDurationUnit { get; set; }
    public string PhoneTokenMinLength { get; set; }
    public string PhoneTokenMaxLength { get; set; }
    public string PhoneTokenValidityDuration { get; set; }
    public string PhoneTokenValidityDurationUnit { get; set; }
    public string LoginFailedThreshold { get; set; }
    public string LockOutThreshold { get; set; }
    public string LockOutDuration { get; set; }
    public string LockOutDurationUnit { get; set; }
    public string SecretCodeValidityDuration { get; set; }
    public string SecretCodeValidityDurationUnit { get; set; }
}

public sealed class SmsContents {
    public string AccountActivationSms { get; set; }
    public string AccountRecoverySms { get; set; }
    public string SecretCodeSms { get; set; }
    public string TwoFactorPinSms { get; set; }
    public string OneTimePasswordSms { get; set; }
    
    public string PhoneNumberConfirmationSms { get; set; }
    public string SecurityQuestionsChangedSms { get; set; }
}