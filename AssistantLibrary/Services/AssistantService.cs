using System.Net.Http.Headers;
using System.Net.Http.Json;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AssistantLibrary.Services; 

public sealed class AssistantService: ServiceBase, IAssistantService {

    private readonly HttpClient _httpClient;

    public AssistantService(
        IEcosystem ecosystem,
        ILoggerService logger,
        IOptions<AssistantLibraryOptions> options
    ): base(ecosystem, logger, options) {
        _httpClient = new HttpClient();
    }
    
    public async Task<RecaptchaResponse?> IsHumanActivity(string clientToken) {
        _logger.Log(new LoggerBinding<AssistantService> { Location = nameof(IsHumanActivity) });

        var (secretKey, endpoint, contentType) = _environment switch {
            Constants.Development => (_options.Value.Dev.RecaptchaSettings.SecretKey, _options.Value.Dev.RecaptchaSettings.Endpoint, _options.Value.Dev.RecaptchaSettings.RequestContentType),
            Constants.Staging => (_options.Value.Stg.RecaptchaSettings.SecretKey, _options.Value.Stg.RecaptchaSettings.Endpoint, _options.Value.Stg.RecaptchaSettings.RequestContentType),
            _ => (_options.Value.Prod.RecaptchaSettings.SecretKey, _options.Value.Prod.RecaptchaSettings.Endpoint, _options.Value.Prod.RecaptchaSettings.RequestContentType)
        };

        _httpClient.BaseAddress = new Uri(endpoint);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypes[contentType]));

        var httpResponse = await _httpClient.PostAsJsonAsync($"?secret={secretKey}&response={clientToken}", HttpCompletionOption.ResponseContentRead);
        return !httpResponse.IsSuccessStatusCode
            ? new RecaptchaResponse()
            : JsonConvert.DeserializeObject<RecaptchaResponse>(await httpResponse.Content.ReadAsStringAsync());
    }
}