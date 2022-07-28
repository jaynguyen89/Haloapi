using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
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
    private readonly Tuple<string, string, string> _defaultBodyPlaceholderValues;

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
            Constants.Production => (options.Value.Prod.MailServiceSettings.DefaultSenderAddress, options.Value.Prod.MailServiceSettings.DefaultSenderName),
            _ => (options.Value.Loc.MailServiceSettings.DefaultSenderAddress, options.Value.Loc.MailServiceSettings.DefaultSenderName)
        };

        _defaultSenderEmailAddress = defaultSenderEmailAddress;
        _defaultSenderName = defaultSenderName;
        
        var (halogenLogoUrl, clientBaseUri, clientApplicationName) = ecosystem.GetEnvironment() switch {
            Constants.Development => (
                options.Value.Dev.MailServiceSettings.DefaultPlaceholderValues.HalogenLogoUrl,
                options.Value.Dev.MailServiceSettings.DefaultPlaceholderValues.ClientBaseUri,
                options.Value.Dev.MailServiceSettings.DefaultPlaceholderValues.ClientApplicationName
            ),
            Constants.Staging => (
                options.Value.Stg.MailServiceSettings.DefaultPlaceholderValues.HalogenLogoUrl,
                options.Value.Stg.MailServiceSettings.DefaultPlaceholderValues.ClientBaseUri,
                options.Value.Stg.MailServiceSettings.DefaultPlaceholderValues.ClientApplicationName
            ),
            Constants.Production => (
                options.Value.Prod.MailServiceSettings.DefaultPlaceholderValues.HalogenLogoUrl,
                options.Value.Prod.MailServiceSettings.DefaultPlaceholderValues.ClientBaseUri,
                options.Value.Prod.MailServiceSettings.DefaultPlaceholderValues.ClientApplicationName
            ),
            _ => (
                options.Value.Loc.MailServiceSettings.DefaultPlaceholderValues.HalogenLogoUrl,
                options.Value.Loc.MailServiceSettings.DefaultPlaceholderValues.ClientBaseUri,
                options.Value.Loc.MailServiceSettings.DefaultPlaceholderValues.ClientApplicationName
            )
        };

        _defaultBodyPlaceholderValues = new Tuple<string, string, string>(halogenLogoUrl, clientBaseUri, clientApplicationName);
        _mailMessage = new MailMessage();
    }
    
    private void ConfigureSmtpClient() {
        var (serverHost, serverPort, useSsl, emailAddress, password) = _environment switch {
            Constants.Development => (
                _options.Dev.MailServiceSettings.MailServerHost,
                int.Parse(_options.Dev.MailServiceSettings.MailServerPort),
                bool.Parse(_options.Dev.MailServiceSettings.UseSsl),
                _options.Dev.MailServiceSettings.ServerCredentails.EmailAddress,
                _options.Dev.MailServiceSettings.ServerCredentails.Password
            ),
            Constants.Staging => (
                _options.Stg.MailServiceSettings.MailServerHost,
                int.Parse(_options.Stg.MailServiceSettings.MailServerPort),
                bool.Parse(_options.Stg.MailServiceSettings.UseSsl),
                _options.Stg.MailServiceSettings.ServerCredentails.EmailAddress,
                _options.Stg.MailServiceSettings.ServerCredentails.Password
            ),
            Constants.Production => (
                _options.Prod.MailServiceSettings.MailServerHost,
                int.Parse(_options.Prod.MailServiceSettings.MailServerPort),
                bool.Parse(_options.Prod.MailServiceSettings.UseSsl),
                _options.Prod.MailServiceSettings.ServerCredentails.EmailAddress,
                _options.Prod.MailServiceSettings.ServerCredentails.Password
            ),
            _ => (
                _options.Loc.MailServiceSettings.MailServerHost,
                int.Parse(_options.Loc.MailServiceSettings.MailServerPort),
                bool.Parse(_options.Loc.MailServiceSettings.UseSsl),
                _options.Loc.MailServiceSettings.ServerCredentails.EmailAddress,
                _options.Loc.MailServiceSettings.ServerCredentails.Password
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

        bodyContent = bodyContent!.SetDefaultEmailBodyValues(_defaultBodyPlaceholderValues);
        _ = mail.Placeholders.Select(x => bodyContent = Regex.Replace(bodyContent!, $@"^{x.Key}$", x.Value));
        _mailMessage.Body = bodyContent;
        
        mail.Attachments?.ForEach(x => _mailMessage.Attachments.Add(new Attachment(x.Key, x.Value)));
        return true;
    }

    private async Task<string?> GetMailBodyContent( Enums.EmailTemplate template) {
        var filePath = $"{Constants.AssetsDirectoryPath}{template.GetEnumValue<string>()}.html";

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