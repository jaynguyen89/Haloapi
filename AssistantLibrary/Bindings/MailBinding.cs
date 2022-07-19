using System.Net.Mail;
using HelperLibrary.Shared;

namespace AssistantLibrary.Bindings; 

public sealed class MailBinding {
    
    /// <summary>
    /// Id is required when sending bulk emails, when an email in the bulk is failed to send, its Id will be returned
    /// </summary>
    public string? Id { get; set; }
    
    public Recipient? Sender { get; set; }

    public List<Recipient> ToReceivers { get; set; } = null!;
    
    public List<Recipient>? CcReceivers { get; set; }
    
    public List<Recipient>? BccReceivers { get; set; }

    public string Title { get; set; } = null!;

    public MailPriority Priority { get; set; } = MailPriority.Normal;
    
    public Enums.EmailTemplate TemplateName { get; set; }

    public Dictionary<string, string> Placeholders { get; set; } = null!;

    public List<KeyValuePair<FileStream, string>>? Attachments { get; set; }
}

public sealed class Recipient {

    public string EmailAddress { get; set; } = null!;

    public string Name { get; set; } = null!;
}