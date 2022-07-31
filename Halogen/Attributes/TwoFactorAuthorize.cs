using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using Halogen.Bindings.ApiBindings;
using Halogen.Parsers;
using Halogen.Services.AppServices.Interfaces;
using HelperLibrary;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Halogen.Attributes; 

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class TwoFactorAuthorize: AuthorizeAttribute, IAuthorizationFilter {

    private readonly bool _twoFactorEnabled;
    private readonly ILoggerService _logger;
    private readonly ISessionService _sessionService;
    private readonly ITwoFactorService _twoFactorService;
    
    public TwoFactorAuthorize(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        ISessionService sessionService,
        ITwoFactorService twoFactorService
    ) {
        _logger = logger;
        _sessionService = sessionService;
        _twoFactorService = twoFactorService;

        var environment = ecosystem.GetEnvironment();
        _twoFactorEnabled = bool.Parse(
            environment switch {
                Constants.Development => configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.ServiceSettings)}{Constants.Colon}{nameof(HalogenOptions.Development.ServiceSettings.RecaptchaEnabled)}"),
                Constants.Staging => configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.ServiceSettings)}{Constants.Colon}{nameof(HalogenOptions.Staging.ServiceSettings.RecaptchaEnabled)}"),
                Constants.Production => configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.ServiceSettings)}{Constants.Colon}{nameof(HalogenOptions.Production.ServiceSettings.RecaptchaEnabled)}"),
                _ => configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.ServiceSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.ServiceSettings.RecaptchaEnabled)}")
            }
        );
    }

    public void OnAuthorization(AuthorizationFilterContext context) {
        _logger.Log(new LoggerBinding<TwoFactorAuthorize> { Location = nameof(OnAuthorization) });
        if (!_twoFactorEnabled) return;

        var twoFactorSecretKey = _sessionService.Get<string>(nameof(HttpHeaderKeys.TwoFactorToken));
        if (!twoFactorSecretKey.IsString())
            context.Result = new UnauthorizedObjectResult(new ClientResponse {
                Result = Enums.ApiResult.FAILED,
                Data = $"{nameof(TwoFactorAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.NO_TWO_FACTOR_TOKEN.GetEnumValue<string>()}"
            });

        var (_, twoFactorToken) = context.HttpContext.Request.Headers.Single(x => x.Key.ToLower().Equals(nameof(HttpHeaderKeys.TwoFactorToken).ToLower()));
        var isTwoFactorTokenValid = _twoFactorService.VerifyTwoFactorAuthenticationPin(new VerifyTwoFactorBinding {
            PinCode = twoFactorToken,
            SecretKey = twoFactorSecretKey!
        });
        
        if (!isTwoFactorTokenValid) context.Result = new UnauthorizedObjectResult(new ClientResponse {
            Result = Enums.ApiResult.FAILED,
            Data = $"{nameof(TwoFactorAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.INVALID_TWO_FACTOR_TOKEN.GetEnumValue<string>()}"
        });
    }
}