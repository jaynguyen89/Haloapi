using AssistantLibrary.Bindings;

namespace AssistantLibrary.Interfaces; 

public interface IMailService {

    /// <summary>
    /// To send 1 email to 1 recipient.
    /// </summary>
    /// <param name="mail">MailBinding</param>
    /// <returns>bool</returns>
    Task<bool> SendSingleEmail(MailBinding mail);

    /// <summary>
    /// To send multiple emails to multiple recipients.
    /// </summary>
    /// <param name="mails">List:MailBinding</param>
    /// <returns>string[]</returns>
    Task<string[]> SendBulkEmails(List<MailBinding> mails);
}