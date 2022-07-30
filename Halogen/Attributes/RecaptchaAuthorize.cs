using AssistantLibrary.Interfaces;
using Halogen.Bindings.ApiBindings;
using Halogen.Parsers;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Halogen.Attributes; 

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class RecaptchaAuthorize: AuthorizeAttribute, IAuthorizationFilter {

    private readonly bool _recaptchaEnabled;
    private readonly ILoggerService _logger;
    private readonly IAssistantService _assistantService;

    internal RecaptchaAuthorize(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        IAssistantService assistantService
    ) {
        _logger = logger;
        _assistantService = assistantService;
        
        var environment = ecosystem.GetEnvironment();
        _recaptchaEnabled = bool.Parse(
            environment switch {
                Constants.Development => configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.ServiceSettings)}{Constants.Colon}{nameof(HalogenOptions.Development.ServiceSettings.TwoFactorEnabled)}"),
                Constants.Staging => configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.ServiceSettings)}{Constants.Colon}{nameof(HalogenOptions.Staging.ServiceSettings.TwoFactorEnabled)}"),
                Constants.Production => configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.ServiceSettings)}{Constants.Colon}{nameof(HalogenOptions.Production.ServiceSettings.TwoFactorEnabled)}"),
                _ => configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.ServiceSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.ServiceSettings.TwoFactorEnabled)}")
            }
        );
    }

    public void OnAuthorization(AuthorizationFilterContext context) {
        _logger.Log(new LoggerBinding<RecaptchaAuthorize> { Location = nameof(OnAuthorization) });
        if (!_recaptchaEnabled) return;
        
        var requestHeaders = context.HttpContext.Request.Headers;
        var (_, recaptchaToken) = requestHeaders.Single(x => x.Key.Equals(nameof(HttpHeaderKeys.RecaptchaToken)));

        var isHuman = _assistantService.IsHumanActivity(recaptchaToken).Result;
        if (isHuman is null || !isHuman.Result)
            context.Result = new UnauthorizedObjectResult(new ClientResponse { Result = Enums.ApiResult.FAILED, Data = nameof(RecaptchaAuthorize) });
    }
}