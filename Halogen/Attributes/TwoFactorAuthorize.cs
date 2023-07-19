using System.Net;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using Halogen.Bindings;
using Halogen.Bindings.ViewModels;
using Halogen.Bindings.ServiceBindings;
using Halogen.Services.AppServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Authorization;
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
        _twoFactorEnabled = bool.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.ServiceSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.ServiceSettings.RecaptchaEnabled)}"));
    }

    public void OnAuthorization(AuthorizationFilterContext context) {
        _logger.Log(new LoggerBinding<TwoFactorAuthorize> { Location = nameof(OnAuthorization) });
        if (!_twoFactorEnabled) return;

        var twoFactorSecretKey = _sessionService.Get<string>(nameof(HttpHeaderKeys.TwoFactorToken));
        if (!twoFactorSecretKey.IsString())
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(TwoFactorAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.NoTwoFactorToken.GetValue()}");

        var (_, twoFactorToken) = context.HttpContext.Request.Headers.Single(x => x.Key.ToLower().Equals(nameof(HttpHeaderKeys.TwoFactorToken).ToLower()));
        var isTwoFactorTokenValid = _twoFactorService.VerifyTwoFactorAuthenticationPin(new VerifyTwoFactorBinding {
            PinCode = twoFactorToken,
            SecretKey = twoFactorSecretKey!
        });
        
        if (!isTwoFactorTokenValid)
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(TwoFactorAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.InvalidTwoFactorToken.GetValue()}");
    }
}