using Halogen.Bindings.ApiBindings;
using Halogen.Bindings.ServiceBindings;
using Halogen.Services.AppServices.Interfaces;
using HelperLibrary;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Halogen.Attributes; 

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class AuthenticatedAuthorize: AuthorizeAttribute, IAuthorizationFilter {

    private readonly ILoggerService _logger;
    private readonly ISessionService _sessionService;

    internal AuthenticatedAuthorize(
        ILoggerService logger,
        ISessionService sessionService
    ) {
        _logger = logger;
        _sessionService = sessionService;
    }

    public void OnAuthorization(AuthorizationFilterContext context) {
        _logger.Log(new LoggerBinding<AuthenticatedAuthorize> { Location = nameof(OnAuthorization) });
        var requestHeaders = context.HttpContext.Request.Headers;

        var authenticatedUser = _sessionService.Get<Authorization>(nameof(Authorization));
        var (_, accountIdFromRequest) = requestHeaders.Single(x => x.Key.ToLower().Equals(nameof(HttpHeaderKeys.AccountId).ToLower()));

        if (!Equals(authenticatedUser!.AccountId, accountIdFromRequest.ToString()))
            context.Result = new UnauthorizedObjectResult(new ClientResponse {
                Result = Enums.ApiResult.FAILED,
                Data = $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.INVALID_USER.GetEnumValue<string>()}"
            });

        var (_, bearerTokenHeader) = requestHeaders.Single(x => x.Key.ToLower().Equals(nameof(HttpHeaderKeys.Authorization).ToLower()));
        var clientBearerToken = bearerTokenHeader.ToString().Split(Constants.MonoSpace).Last();
        
        if (!Equals(authenticatedUser.BearerToken, clientBearerToken))
            context.Result = new UnauthorizedObjectResult(new ClientResponse {
                Result = Enums.ApiResult.FAILED,
                Data = $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.MISMATCHED_BEARER_TOKEN.GetEnumValue<string>()}"
            });

        var (_, clientAuthorizationToken) = requestHeaders.Single(x => x.Key.ToLower().Equals(nameof(HttpHeaderKeys.AuthorizationToken).ToLower()));
        if (!Equals(authenticatedUser.AuthorizationToken, clientAuthorizationToken.ToString()))
            context.Result = new UnauthorizedObjectResult(new ClientResponse {
                Result = Enums.ApiResult.FAILED,
                Data = $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.MISMATCHED_AUTH_TOKEN.GetEnumValue<string>()}"
            });
        
        if (authenticatedUser.AuthorizedTimestamp + authenticatedUser.ValidityDuration < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            context.Result = new UnauthorizedObjectResult(new ClientResponse {
                Result = Enums.ApiResult.FAILED,
                Data = $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.AUTHORIZATION_EXPIRED.GetEnumValue<string>()}"
            });
    }
}