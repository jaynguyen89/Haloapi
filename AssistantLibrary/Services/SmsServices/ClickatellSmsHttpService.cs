using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces.SmsServices;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AssistantLibrary.Services.SmsServices;

public sealed class ClickatellSmsHttpService: ServiceBase, IClickatellSmsHttpService {

    private readonly HttpClient _httpClient;
    private readonly string _clickatellBaseUrl;
    
    public ClickatellSmsHttpService(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ): base(ecosystem, logger, configuration) {
        _httpClient = new HttpClient();

        var (clickatellEndpoint, clickatellApiKey) = (_assistantConfigs.ClickatellSettings.HttpEndpoint, _assistantConfigs.ClickatellSettings.ApiKey);
        _clickatellBaseUrl = $"{clickatellEndpoint}?{nameof(clickatellApiKey)}={clickatellApiKey}";
    }

    public async Task<string[]?> SendSingleSms(SingleSmsBinding binding) {
        _logger.Log(new LoggerBinding<ClickatellSmsHttpService> { Location = nameof(SendSingleSms) });

        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypes[_assistantConfigs.ClickatellSettings.RequestContentType]));

        var smsContent = Regex.Replace(binding.SmsContent, $@"\bCLIENT_APPLICATION_NAME\b", _assistantConfigs.MailServiceSettings.PlaceholderClientAppName);
        var encodedContent = Uri.EscapeDataString(smsContent);
        if (_environment.Equals(Constants.Local)) {
            var receiverPhoneNumber = _assistantConfigs.ClickatellSettings.PhoneNumber;
            var requestUrl = $"{_clickatellBaseUrl}&to={receiverPhoneNumber}&content={encodedContent}";
            
            _httpClient.BaseAddress = new Uri(requestUrl);
            var httpResponse = await _httpClient.GetAsync($"", HttpCompletionOption.ResponseContentRead);

            return httpResponse.IsSuccessStatusCode ? default : [receiverPhoneNumber];
        }

        try {
            var smsSendingResults = binding.Receivers.Select(async x => {
                                               var requestUrl = $"{_clickatellBaseUrl}&to={x}&content={encodedContent}";
                                               _httpClient.BaseAddress = new Uri(requestUrl);

                                               var httpResponse = await _httpClient.GetAsync(string.Empty, HttpCompletionOption.ResponseContentRead);
                                               return httpResponse.IsSuccessStatusCode ? string.Empty : x;
                                           })
                                           .Select(x => x.Result)
                                           .AsQueryable();

            return await smsSendingResults.Where(x => x.IsString())
                                          .ToArrayAsync();
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<ClickatellSmsHttpService> { Location = nameof(SendSingleSms), Severity = Enums.LogSeverity.Error, E = e });
            return default;
        }
    }

    public async Task<string[]?> SendMultipleSms(MultipleSmsBinding bindings) {
        _logger.Log(new LoggerBinding<ClickatellSmsHttpService> { Location = nameof(SendMultipleSms) });

        try {
            var smsSendingResults = bindings.SmsBindings
                                            .Select(async x => await SendSingleSms(x))
                                            .Select(x => x.Result)
                                            .AsQueryable();

            return await smsSendingResults.Where(x => x != null)
                                          .SelectMany(x => x!)
                                          .ToArrayAsync();
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<ClickatellSmsHttpService> { Location = nameof(SendMultipleSms), Severity = Enums.LogSeverity.Error, E = e });
            return default;
        }
    }
}