namespace AssistantLibrary.Bindings; 

public sealed class SingleSmsBinding {

    public string SmsContent { get; set; } = null!;

    public List<string> Receivers { get; set; } = null!;
}

public sealed class MultipleSmsBinding {

    public List<SingleSmsBinding> SmsBindings { get; set; } = null!;
}