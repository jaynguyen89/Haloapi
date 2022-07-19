using AssistantLibrary.Bindings;

namespace AssistantLibrary.Interfaces; 

public interface IMailService {

    Task<bool> SendSingleEmail(MailBinding mail);

    Task<List<string>?> SendBulkEmails(List<MailBinding> mails);
}