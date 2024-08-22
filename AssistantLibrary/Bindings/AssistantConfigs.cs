using QRCoder;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace AssistantLibrary.Bindings;

public sealed class AssistantConfigs {
    
    public string ProjectName { get; set; }
    
    public int RsaKeyLength { get; set; }
    
    public TwoFactorSettings TwoFactorSettings { get; set; }
    
    public RecaptchaSettings RecaptchaSettings { get; set; }
    
    public MailServiceSettings MailServiceSettings { get; set; }
    
    public ServiceFactorySettings ServiceFactorySettings { get; set; }
    
    public ClickatellSettings ClickatellSettings { get; set; }
    
    public QrGeneratorSettings QrGeneratorSettings { get; set; }
}

public sealed class TwoFactorSettings {
    public int QrImageSize { get; set; }
    public int ToleranceDuration { get; set; }
}

public sealed class RecaptchaSettings {
    public string SecretKey { get; set; }
    public string Endpoint { get; set; }
    public string RequestContentType { get; set; }
}

public sealed class MailServiceSettings {
    public string MailServerHost { get; set; }
    public int MailServerPort { get; set; }
    public bool UseSsl { get; set; }
    public int Timeout { get; set; }
    public string EmailAddressCredential { get; set; } // Sender's email address
    public string PasswordCredential { get; set; } // Sender's email password
    public string DefaultSenderEmailAddress { get; set; }
    public string DefaultSenderName { get; set; }
    public string PlaceholderHalogenLogoUrl { get; set; }
    public string PlaceholderClientBaseUri { get; set; }
    public string PlaceholderClientAppName { get; set; }
}

public sealed class ServiceFactorySettings {
    public string ActiveMailService { get; set; }
    public string ActiveSmsService { get; set; }
    public string ActiveTfaService { get; set; }
}

public sealed class ClickatellSettings {
    public string ApiKey { get; set; }
    public string HttpEndpoint { get; set; }
    public string PhoneNumber { get; set; }
    public string RequestContentType { get; set; }
}

public sealed class QrGeneratorSettings {
    public int ImageSize { get; set; }
    public string DarkColor { get; set; }
    public string LightColor { get; set; }
    public QRCodeGenerator.ECCLevel EccLevel { get; set; }
    public bool WithLogo { get; set; }
    public string LogoName { get; set; }
}
