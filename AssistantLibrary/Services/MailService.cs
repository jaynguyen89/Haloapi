using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using HelperLibrary;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Configuration;

namespace AssistantLibrary.Services; 

public sealed class MailService: ServiceBase, IMailService {

    private readonly SmtpClient _smtpClient;
    private readonly string _defaultSenderEmailAddress;
    private readonly string _defaultSenderName;
    private MailMessage _mailMessage;
    private readonly Tuple<string, string> _defaultBodyPlaceholderValues;

    public MailService(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ): base(ecosystem, logger, configuration) {
        _smtpClient = new SmtpClient();
        ConfigureSmtpClient();
        
        var (defaultSenderEmailAddress, defaultSenderName) = (
            _configuration.AsEnumerable().Single(x => x.Key.Equals($"{_mailServiceBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.DefaultSenderAddress)}")).Value,
            _configuration.AsEnumerable().Single(x => x.Key.Equals($"{_mailServiceBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.DefaultSenderName)}")).Value
        );

        _defaultSenderEmailAddress = defaultSenderEmailAddress;
        _defaultSenderName = defaultSenderName;

        var defaultPlaceholdersBaseOptionKey = $"{_mailServiceBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.DefaultPlaceholders)}{Constants.Colon}";
        var (halogenLogoUrl, clientBaseUri) = (
            _configuration.AsEnumerable().Single(x => x.Key.Equals($"{defaultPlaceholdersBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.DefaultPlaceholders.HalogenLogoUrl)}")).Value,
            _configuration.AsEnumerable().Single(x => x.Key.Equals($"{defaultPlaceholdersBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.DefaultPlaceholders.ClientBaseUri)}")).Value
        );

        _defaultBodyPlaceholderValues = new Tuple<string, string>(halogenLogoUrl, clientBaseUri);
        _mailMessage = new MailMessage();
    }
    
    private void ConfigureSmtpClient() {
        var serverCredentialsBaseOptionKey = $"{_mailServiceBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.ServerCredentials)}{Constants.Colon}";
        var (serverHost, serverPort, useSsl, timeout, emailAddress, password) = (
            _configuration.AsEnumerable().Single(x => x.Key.Equals($"{_mailServiceBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.MailServerHost)}")).Value,
            int.Parse(_configuration.AsEnumerable().Single(x => x.Key.Equals($"{_mailServiceBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.MailServerPort)}")).Value),
            bool.Parse(_configuration.AsEnumerable().Single(x => x.Key.Equals($"{_mailServiceBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.UseSsl)}")).Value),
            int.Parse(_configuration.AsEnumerable().Single(x => x.Key.Equals($"{_mailServiceBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.Timeout)}")).Value),
            _configuration.AsEnumerable().Single(x => x.Key.Equals($"{serverCredentialsBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.ServerCredentials.EmailAddress)}")).Value,
            _configuration.AsEnumerable().Single(x => x.Key.Equals($"{serverCredentialsBaseOptionKey}{nameof(AssistantLibraryOptions.Local.MailServiceSettings.ServerCredentials.Password)}")).Value
        );

        _smtpClient.Host = serverHost;
        _smtpClient.Port = serverPort;
        _smtpClient.EnableSsl = useSsl;
        _smtpClient.UseDefaultCredentials = useSsl;
        _smtpClient.Credentials = new NetworkCredential(emailAddress, password);
        _smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        _smtpClient.Timeout = timeout;
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

        bodyContent = bodyContent!.SetDefaultEmailBodyValues(new Tuple<string, string, string>(_defaultBodyPlaceholderValues.Item1, _defaultBodyPlaceholderValues.Item2, _clientApplicationName));
        _ = mail.Placeholders.Select(x => bodyContent = Regex.Replace(bodyContent!, $@"^{x.Key}$", x.Value));
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
        catch {
            return false;
        }
    }

    public async Task<string[]> SendBulkEmails(List<MailBinding> mails) {
        _logger.Log(new LoggerBinding<MailService> { Location = nameof(SendBulkEmails) });

        var emailsFailedToSend = new List<string>();
        foreach (var mail in mails) {
            var isSent = await SendSingleEmail(mail);
            if (!isSent) emailsFailedToSend.Add(mail.Id!);
        }

        return emailsFailedToSend.ToArray();
    }
}