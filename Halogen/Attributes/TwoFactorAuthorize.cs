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
using Microsoft.Extensions.Options;

namespace Halogen.Attributes; 

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class TwoFactorAuthorize: AuthorizeAttribute, IAuthorizationFilter {

    private readonly bool _twoFactorEnabled;
    private readonly ILoggerService _logger;
    private readonly ISessionService _sessionService;
    private readonly ITwoFactorService _twoFactorService;
    
    internal TwoFactorAuthorize(
        IEcosystem ecosystem,
        ILoggerService logger,
        IOptions<HalogenOptions> options,
        ISessionService sessionService,
        ITwoFactorService twoFactorService
    ) {
        _logger = logger;
        _sessionService = sessionService;
        _twoFactorService = twoFactorService;

        _twoFactorEnabled = bool.Parse(
            ecosystem.GetEnvironment() switch {
                Constants.Development => options.Value.Dev.ServiceSettings.TwoFactorEnabled,
                Constants.Staging => options.Value.Stg.ServiceSettings.TwoFactorEnabled,
                Constants.Production => options.Value.Prod.ServiceSettings.TwoFactorEnabled,
                _ => options.Value.Loc.ServiceSettings.TwoFactorEnabled
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