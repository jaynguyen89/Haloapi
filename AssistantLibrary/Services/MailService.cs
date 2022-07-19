using System.Net;
using System.Net.Mail;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using HelperLibrary;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Options;

namespace AssistantLibrary.Services; 

public sealed class MailService: ServiceBase, IMailService {

    private readonly SmtpClient _smtpClient;
    private readonly string _defaultSenderEmailAddress;
    private readonly string _defaultSenderName;
    private MailMessage _mailMessage;

    public MailService(
        IEcosystem ecosystem,
        ILoggerService logger,
        IOptions<AssistantLibraryOptions> options
    ): base(ecosystem, logger, options) {
        _smtpClient = new SmtpClient();
        ConfigureSmtpClient();

        var (defaultSenderEmailAddress, defaultSenderName) = ecosystem.GetEnvironment() switch {
            Constants.Development => (options.Value.Dev.MailServiceSettings.DefaultSenderAddress, options.Value.Dev.MailServiceSettings.DefaultSenderName),
            Constants.Staging => (options.Value.Stg.MailServiceSettings.DefaultSenderAddress, options.Value.Stg.MailServiceSettings.DefaultSenderName),
            _ => (options.Value.Prod.MailServiceSettings.DefaultSenderAddress, options.Value.Prod.MailServiceSettings.DefaultSenderName)
        };

        _defaultSenderEmailAddress = defaultSenderEmailAddress;
        _defaultSenderName = defaultSenderName;

        _mailMessage = new MailMessage();
    }
    
    private void ConfigureSmtpClient() {
        var (serverHost, serverPort, useSsl, emailAddress, password) = _environment switch {
            Constants.Development => (
                _options.Value.Dev.MailServiceSettings.MailServerHost,
                int.Parse(_options.Value.Dev.MailServiceSettings.MailServerPort),
                bool.Parse(_options.Value.Dev.MailServiceSettings.UseSsl),
                _options.Value.Dev.MailServiceSettings.ServerCredentails.EmailAddress,
                _options.Value.Dev.MailServiceSettings.ServerCredentails.Password
            ),
            Constants.Staging => (
                _options.Value.Stg.MailServiceSettings.MailServerHost,
                int.Parse(_options.Value.Stg.MailServiceSettings.MailServerPort),
                bool.Parse(_options.Value.Stg.MailServiceSettings.UseSsl),
                _options.Value.Stg.MailServiceSettings.ServerCredentails.EmailAddress,
                _options.Value.Stg.MailServiceSettings.ServerCredentails.Password
            ),
            _ => (
                _options.Value.Prod.MailServiceSettings.MailServerHost,
                int.Parse(_options.Value.Prod.MailServiceSettings.MailServerPort),
                bool.Parse(_options.Value.Prod.MailServiceSettings.UseSsl),
                _options.Value.Prod.MailServiceSettings.ServerCredentails.EmailAddress,
                _options.Value.Prod.MailServiceSettings.ServerCredentails.Password
            )
        };

        _smtpClient.Host = serverHost;
        _smtpClient.Port = serverPort;
        _smtpClient.EnableSsl = useSsl;
        _smtpClient.UseDefaultCredentials = useSsl;
        _smtpClient.Credentials = new NetworkCredential(emailAddress, password);
    }

    private async Task<bool> Compose(MailBinding mail) {
        var bodyContent = await GetMailBodyContent(mail.TemplateName);
        if (!bodyContent.IsString()) return false;
        
        var (fromAddress, fromName) = mail.Sender is not null
            ? (mail.Sender.EmailAddress, mail.Sender.Name)
            : (_defaultSenderEmailAddress, _defaultSenderName);
        
        _mailMessage = new MailMessage { From = new MailAddress(fromAddress, fromName) };
        
        mail.ToReceivers.ForEach(x => _mailMessage.To.Add(new MailAddress(x.EmailAddress, x.Name)));
        mail.CcReceivers?.ForEach(x => _mailMessage.CC.Add(new MailAddress(x.EmailAddress, x.Name)));
        mail.BccReceivers?.ForEach(x => _mailMessage.Bcc.Add(new MailAddress(x.EmailAddress, x.Name)));

        _mailMessage.Subject = mail.Title;
        _mailMessage.IsBodyHtml = true;
        _mailMessage.Priority = mail.Priority;

        _ = mail.Placeholders.Select(x => bodyContent = bodyContent.Replace(x.Key, x.Value));
        _mailMessage.Body = bodyContent;
        
        mail.Attachments?.ForEach(x => _mailMessage.Attachments.Add(new Attachment(x.Key, x.Value)));
        return true;
    }

    private async Task<string?> GetMailBodyContent( Enums.EmailTemplate template) {
        var filePath = template switch {
            _ => $"{Constants.EmailTemplateFolderPath}template.html"
        };

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
        catch {
            return false;
        }
    }

    public async Task<List<string>?> SendBulkEmails(List<MailBinding> mails) {
        _logger.Log(new LoggerBinding<MailService> { Location = nameof(SendBulkEmails) });

        var emailsFailedToSend = new List<string>();
        foreach (var mail in mails) {
            var isComposed = await Compose(mail);
            if (!isComposed) {
                emailsFailedToSend.Add(mail.Id!);
                continue;
            }

            try {
                await _smtpClient.SendMailAsync(_mailMessage);
            }
            catch {
                emailsFailedToSend.Add(mail.Id!);
            }
        }

        return emailsFailedToSend.Any() ? emailsFailedToSend : default;
    }
}