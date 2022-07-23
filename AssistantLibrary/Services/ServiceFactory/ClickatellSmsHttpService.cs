﻿using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces.IServiceFactory;
using HelperLibrary;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace AssistantLibrary.Services.ServiceFactory;

public sealed class ClickatellSmsHttpService: ServiceBase, ISmsService, IClickatellSmsHttpService {

    private readonly HttpClient _httpClient;
    private readonly string _clickatellBaseUrl;
    
    public ClickatellSmsHttpService(
        IEcosystem ecosystem,
        ILoggerService logger,
        IOptions<AssistantLibraryOptions> options,
        string clickatellBaseUrl
    ): base(ecosystem, logger, options) {
        _httpClient = new HttpClient();
        _clickatellBaseUrl = clickatellBaseUrl;
    }

    public async Task<string[]?> SendSingleSms(SingleSmsBinding binding) {
        _logger.Log(new LoggerBinding<ClickatellSmsHttpService> { Location = nameof(SendSingleSms) });
        
        var contentType = _environment switch {
            Constants.Development => _options.Value.Dev.ClickatellHttpSettings.RequestContentType,
            Constants.Staging => _options.Value.Stg.ClickatellHttpSettings.RequestContentType,
            _ => _options.Value.Prod.ClickatellHttpSettings.RequestContentType
        };
        
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypes[contentType]));

        var encodedContent = Regex.Replace(binding.SmsContent, Constants.MonoSpace, Constants.Plus);
        if (_environment.Equals(Constants.Development)) {
            var receiverPhoneNumber = _options.Value.Dev.ClickatellHttpSettings.DevTestPhoneNumber;
            var requestUrl = $"{_clickatellBaseUrl}&to={receiverPhoneNumber}&content={encodedContent}";
            
            _httpClient.BaseAddress = new Uri(requestUrl);
            var httpResponse = await _httpClient.GetAsync($"", HttpCompletionOption.ResponseContentRead);

            return httpResponse.IsSuccessStatusCode ? default : new [] {receiverPhoneNumber};
        }

        try {
            var smsSendingResults = binding.Receivers.Select(async x => {
                                               var requestUrl = $"{_clickatellBaseUrl}&to={x}&content={encodedContent}";
                                               _httpClient.BaseAddress = new Uri(requestUrl);

                                               var httpResponse = await _httpClient.GetAsync($"", HttpCompletionOption.ResponseContentRead);
                                               return httpResponse.IsSuccessStatusCode ? string.Empty : x;

                                           })
                                           .Select(x => x.Result)
                                           .AsQueryable();

            return await smsSendingResults.Where(x => x.IsString())
                                          .ToArrayAsync();
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<ClickatellSmsHttpService> { Location = nameof(SendSingleSms), Severity = Enums.LogSeverity.ERROR, Data = e });
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
            _logger.Log(new LoggerBinding<ClickatellSmsHttpService> { Location = nameof(SendMultipleSms), Severity = Enums.LogSeverity.ERROR, Data = e });
            return default;
        }
    }
}