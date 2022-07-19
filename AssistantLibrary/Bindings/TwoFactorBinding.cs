using HelperLibrary.Shared;

namespace AssistantLibrary.Bindings; 

public class TwoFactorBinding {

    public string SecretKey { get; set; } = null!;
}

public sealed class GetTwoFactorBinding: TwoFactorBinding {

    public string EmailAddress { get; set; } = null!;

    public string ProjectName { get; set; } = Constants.ProjectName;

    public byte ImageSize { get; set; } = byte.MaxValue;
}

public sealed class VerifyTwoFactorBinding : TwoFactorBinding {

    public string PinCode { get; set; } = null!;
}

public sealed class TwoFactorData {

    public string QrCodeImageUrl { get; set; } = null!;

    public string ManualEntryKey { get; set; } = null!;
}