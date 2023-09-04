﻿using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces.IServiceFactory;
using AssistantLibrary.Interfaces.SmsServices;
using HelperLibrary;
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
        IConfiguration configuration,
        string clickatellBaseUrl
    ): base(ecosystem, logger, configuration) {
        _httpClient = new HttpClient();
        _clickatellBaseUrl = clickatellBaseUrl;
    }

    public async Task<string[]?> SendSingleSms(SingleSmsBinding binding) {
        _logger.Log(new LoggerBinding<ClickatellSmsHttpService> { Location = nameof(SendSingleSms) });

        var contentType = _configuration.AsEnumerable().Single(x => x.Key.Equals($"{_clickatellBaseOptionKey}{nameof(AssistantLibraryOptions.Local.ClickatellHttpSettings.RequestContentType)}")).Value;
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypes[contentType]));

        var smsContent = Regex.Replace(binding.SmsContent, $@"\bCLIENT_APPLICATION_NAME\b", _clientApplicationName);
        var encodedContent = Uri.EscapeDataString(smsContent); //Regex.Replace(smsContent, Constants.MonoSpace, Constants.Plus);
        if (_environment.Equals(Constants.Local)) {
            var receiverPhoneNumber = _configuration.AsEnumerable().Single(x => x.Key.Equals($"{_clickatellBaseOptionKey}{nameof(AssistantLibraryOptions.Local.ClickatellHttpSettings.DevTestPhoneNumber)}")).Value;
            var requestUrl = $"{_clickatellBaseUrl}&to={receiverPhoneNumber}&content={encodedContent}";
            
            _httpClient.BaseAddress = new Uri(requestUrl);
            var httpResponse = await _httpClient.GetAsync($"", HttpCompletionOption.ResponseContentRead);

            return httpResponse.IsSuccessStatusCode ? default : new [] {receiverPhoneNumber};
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
            _logger.Log(new LoggerBinding<ClickatellSmsHttpService> { Location = nameof(SendSingleSms), Severity = Enums.LogSeverity.Error, Data = e });
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
            _logger.Log(new LoggerBinding<ClickatellSmsHttpService> { Location = nameof(SendMultipleSms), Severity = Enums.LogSeverity.Error, Data = e });
            return default;
        }
    }
}