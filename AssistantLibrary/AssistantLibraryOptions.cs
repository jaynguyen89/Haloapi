﻿#pragma warning disable 8618
namespace AssistantLibrary;
    
public sealed class AssistantLibraryOptions {

    public class Local {
        public string? ProjectName { get; set; }
        public string RsaKeyLength { get; set; }
        public TwoFactorSettings TwoFactorSettings { get; set; }
        public RecaptchaSettings RecaptchaSettings { get; set; }
        public MailServiceSettings MailServiceSettings { get; set; }
        public ServiceFactorySettings ServiceFactorySettings { get; set; }
        public ClickatellHttpSettings ClickatellHttpSettings { get; set; }
        public QrGeneratorSettings QrGeneratorSettings { get; set; }
    }
    
    public sealed class Development: Local { }
    
    public sealed class Staging: Local { }

    public sealed class Production: Local { }
}

public sealed class TwoFactorSettings {
    public string? QrImageSize { get; set; }
    public string ToleranceDuration { get; set; }
}

public sealed class RecaptchaSettings {
    public string SecretKey { get; set; }
    public string Endpoint { get; set; }
    public string RequestContentType { get; set; }
}

public sealed class MailServiceSettings {
    public string MailServerHost { get; set; }
    public string MailServerPort { get; set; }
    public string UseSsl { get; set; }
    public string Timeout { get; set; }
    public Credentials ServerCredentials { get; set; }
    public string DefaultSenderAddress { get; set; }
    public string DefaultSenderName { get; set; }
    public DefaultPlaceholders DefaultPlaceholders { get; set; }
}

public sealed class Credentials {
    public string EmailAddress { get; set; }
    public string Password { get; set; }
}

public sealed class DefaultPlaceholders {
    public string HalogenLogoUrl { get; set; }
    public string ClientBaseUri { get; set; }
    public string ClientApplicationName { get; set; }
}

public sealed class ServiceFactorySettings {
    public string ActiveMailService { get; set; }
    public string ActiveSmsService { get; set; }
    public string ActiveTfaService { get; set; }
}

public sealed class ClickatellHttpSettings {
    public string HttpEndpoint { get; set; }
    public string ApiKey { get; set; }
    public string DevTestPhoneNumber { get; set; }
    public string RequestContentType { get; set; }
}

public sealed class QrGeneratorSettings {
    public string ImageSize { get; set; }
    public string DarkColor { get; set; }
    public string LightColor { get; set; }
    public string EccLevel { get; set; }
    public string WithLogo { get; set; }
    public string LogoName { get; set; }
}