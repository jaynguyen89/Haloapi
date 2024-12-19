using HelperLibrary.Shared;

namespace Halogen.Bindings.ServiceBindings;

public sealed class NotificationContent {

    public string Title { get; set; } = null!;
    
    public Enums.EmailTemplate MailTemplateName { get; set; }
    
    public string? SmsContent { get; set; }

    public Dictionary<string, string>? Placeholders { get; set; }
}