using AssistantLibrary.Bindings;

namespace AssistantLibrary.Interfaces; 

public interface IMailService {

    Task<bool> SendSingleEmail(MailBinding mail);

    Task<string[]> SendBulkEmails(List<MailBinding> mails);
}