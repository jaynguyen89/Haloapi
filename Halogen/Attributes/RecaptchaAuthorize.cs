using System.Net;
using AssistantLibrary;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Services;
using Halogen.Bindings;
using Halogen.Bindings.ServiceBindings;
using Halogen.Bindings.ViewModels;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Halogen.Attributes; 

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RecaptchaAuthorize: AuthorizeAttribute, IAuthorizationFilter {

    private readonly bool _recaptchaEnabled;
    private readonly ILoggerService _logger;
    private readonly IAssistantService _assistantService;

    public RecaptchaAuthorize(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        IAssistantServiceFactory assistantServiceFactory
    ) {
        _logger = logger;
        _assistantService = assistantServiceFactory.GetService<AssistantService>() ?? new AssistantService(ecosystem, logger, configuration);

        var environment = ecosystem.GetEnvironment();
        _recaptchaEnabled = bool.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.ServiceSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.ServiceSettings.RecaptchaEnabled)}")!);
    }

    public void OnAuthorization(AuthorizationFilterContext context) {
        _logger.Log(new LoggerBinding<RecaptchaAuthorize> { Location = nameof(OnAuthorization) });
        if (!_recaptchaEnabled) return;
        
        var requestHeaders = context.HttpContext.Request.Headers;
        var (_, recaptchaToken) = requestHeaders.Single(x => x.Key.Equals(nameof(HttpHeaderKeys.RecaptchaToken)));

        try {
            var recaptchaResponse = _assistantService.IsHumanActivity(recaptchaToken).Result;
            if (!recaptchaResponse.IsHuman)
                context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.RecaptchaNotAHuman.GetValue()}");
        } catch (Exception) {
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.NoRecaptchaToken.GetValue()}");
        }
    }
}