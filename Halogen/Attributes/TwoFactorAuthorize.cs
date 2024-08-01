using System.Net;
using AssistantLibrary;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Services;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings;
using Halogen.Bindings.ServiceBindings;
using Halogen.Bindings.ViewModels;
using Halogen.Services.AppServices.Interfaces;
using Halogen.Services.AppServices.Services;
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
        IHaloServiceFactory haloServiceFactory,
        IAssistantServiceFactory assistantServiceFactory
    ) {
        _logger = logger;
        
        _sessionService = haloServiceFactory.GetService<SessionService>(Enums.ServiceType.AppService) ?? throw new HaloArgumentNullException<TwoFactorAuthorize>(nameof(SessionService));
        _twoFactorService = assistantServiceFactory.GetService<TwoFactorService>() ?? throw new HaloArgumentNullException<TwoFactorAuthorize>(nameof(TwoFactorService));

        var environment = ecosystem.GetEnvironment();
        _twoFactorEnabled = bool.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.ServiceSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.ServiceSettings.TwoFactorEnabled)}")!);
    }

    public void OnAuthorization(AuthorizationFilterContext context) {
        _logger.Log(new LoggerBinding<TwoFactorAuthorize> { Location = nameof(OnAuthorization) });
        if (!_twoFactorEnabled) return;

        var twoFactorSecretKey = _sessionService.Get<string>(nameof(HttpHeaderKeys.TwoFactorToken));
        if (!twoFactorSecretKey.IsString()) {
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(TwoFactorAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.NoTwoFactorToken.GetValue()}");
            return;
        }
            
        var (_, twoFactorToken) = context.HttpContext.Request.Headers.Single(x => x.Key.ToLower().Equals(nameof(HttpHeaderKeys.TwoFactorToken).ToLower()));
        var isTwoFactorTokenValid = _twoFactorService.VerifyTwoFactorAuthenticationPin(new VerifyTwoFactorBinding {
            PinCode = twoFactorToken,
            SecretKey = twoFactorSecretKey!
        });
        
        if (!isTwoFactorTokenValid)
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(TwoFactorAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.InvalidTwoFactorToken.GetValue()}");
    }
}