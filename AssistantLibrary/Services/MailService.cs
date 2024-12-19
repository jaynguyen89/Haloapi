using System.Net;
using System.Net.Mail;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Configuration;

namespace AssistantLibrary.Services; 

public sealed class MailService: ServiceBase, IMailService {

    private readonly SmtpClient _smtpClient;
    private MailMessage _mailMessage;
    private readonly Tuple<string, string> _defaultBodyPlaceholderValues;

    public MailService(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ): base(ecosystem, logger, configuration) {
        _smtpClient = new SmtpClient();
        ConfigureSmtpClient();

        _defaultBodyPlaceholderValues = new Tuple<string, string>(_assistantConfigs.MailServiceSettings.PlaceholderHalogenLogoUrl, _assistantConfigs.MailServiceSettings.PlaceholderClientBaseUri);
        _mailMessage = new MailMessage();
    }
    
    private void ConfigureSmtpClient() {
        _smtpClient.Host = _assistantConfigs.MailServiceSettings.MailServerHost;
        _smtpClient.Port = _assistantConfigs.MailServiceSettings.MailServerPort;
        _smtpClient.EnableSsl = _assistantConfigs.MailServiceSettings.UseSsl;
        _smtpClient.UseDefaultCredentials = !_assistantConfigs.MailServiceSettings.UseSsl;
        _smtpClient.Credentials = new NetworkCredential(_assistantConfigs.MailServiceSettings.EmailAddressCredential, _assistantConfigs.MailServiceSettings.PasswordCredential);
        _smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        _smtpClient.Timeout = _assistantConfigs.MailServiceSettings.Timeout;
    }

    private async Task<bool> Compose(MailBinding mail) {
        var bodyContent = await GetMailBodyContent(mail.TemplateName);
        if (!bodyContent.IsString()) return false;
        
        var (fromAddress, fromName) = mail.Sender is not null
            ? (mail.Sender.EmailAddress, mail.Sender.Name)
            : (_assistantConfigs.MailServiceSettings.DefaultSenderEmailAddress, _assistantConfigs.MailServiceSettings.DefaultSenderName);
        
        _mailMessage = new MailMessage { From = new MailAddress(fromAddress, fromName) };
        
        mail.ToReceivers.ForEach(x => _mailMessage.To.Add(new MailAddress(x.EmailAddress, x.Name)));
        mail.CcReceivers?.ForEach(x => _mailMessage.CC.Add(new MailAddress(x.EmailAddress, x.Name)));
        mail.BccReceivers?.ForEach(x => _mailMessage.Bcc.Add(new MailAddress(x.EmailAddress, x.Name)));

        _mailMessage.Subject = mail.Title;
        _mailMessage.IsBodyHtml = true;
        _mailMessage.Priority = mail.Priority;

        bodyContent = bodyContent!.SetDefaultEmailBodyValues(new Tuple<string, string, string>(_defaultBodyPlaceholderValues.Item1, _defaultBodyPlaceholderValues.Item2, _assistantConfigs.MailServiceSettings.PlaceholderClientAppName));
        bodyContent = mail.Placeholders.Aggregate(bodyContent, (current, entry) => current.Replace(entry.Key, entry.Value));
        _mailMessage.Body = bodyContent;
        
        mail.Attachments?.ForEach(x => _mailMessage.Attachments.Add(new Attachment(x.Key, x.Value)));
        return true;
    }

    private async Task<string?> GetMailBodyContent( Enums.EmailTemplate template) {
        var filePath = $"{Constants.AssetsDirectoryPath}{template.GetValue()}.html";

        try {
            return await File.ReadAllTextAsync(filePath);
        }
        catch {
            return default;
        }
    }

    public async Task<bool> SendSingleEmail(MailBinding mail) {
        _logger.Log(new LoggerBinding<MailService> { Location = nameof(SendSingleEmail) });

        var isComposed = await Compose(mail);
        if (!isComposed) return false;

        try {
            await _smtpClient.SendMailAsync(_mailMessage);
            return true;
        }
        catch (Exception e) {
            _logger.Log(new LoggerBinding<MailService> {
                Location = $"{nameof(SendSingleEmail)}.{nameof(Exception)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return false;
        }
    }

    public async Task<string[]> SendBulkEmails(List<MailBinding> mails) {
        _logger.Log(new LoggerBinding<MailService> { Location = nameof(SendBulkEmails) });

        var emailsFailedToSend = new List<string>();
        foreach (var mail in mails) {
            if (mail.Id is null) throw new PropertyNullException("Mail ID Null: the mail ID is required in bulk mailing.");
            
            var isSent = await SendSingleEmail(mail);
            if (!isSent) emailsFailedToSend.Add(mail.Id);
        }

        return emailsFailedToSend.ToArray();
    }
}