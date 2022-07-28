using AssistantLibrary.Interfaces;
using Halogen.Bindings.ApiBindings;
using Halogen.Parsers;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Halogen.Attributes; 

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class RecaptchaAuthorize: AuthorizeAttribute, IAuthorizationFilter {

    private readonly bool _recaptchaEnabled;
    private readonly ILoggerService _logger;
    private readonly IAssistantService _assistantService;

    internal RecaptchaAuthorize(
        IEcosystem ecosystem,
        ILoggerService logger,
        IOptions<HalogenOptions> options,
        IAssistantService assistantService
    ) {
        _logger = logger;
        _assistantService = assistantService;
        
        _recaptchaEnabled = bool.Parse(
            ecosystem.GetEnvironment() switch {
                Constants.Development => options.Value.Dev.ServiceSettings.RecaptchaEnabled,
                Constants.Staging => options.Value.Stg.ServiceSettings.RecaptchaEnabled,
                Constants.Production => options.Value.Prod.ServiceSettings.RecaptchaEnabled,
                _ => options.Value.Loc.ServiceSettings.RecaptchaEnabled
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