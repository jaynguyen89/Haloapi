using System.Net;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings;
using Halogen.Bindings.ServiceBindings;
using Halogen.Bindings.ViewModels;
using Halogen.Services.AppServices.Interfaces;
using Halogen.Services.AppServices.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Authorization = Halogen.Bindings.ServiceBindings.Authorization;

namespace Halogen.Attributes; 

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AuthenticatedAuthorize: AuthorizeAttribute, IAuthorizationFilter {

    private readonly ILoggerService _logger;
    private readonly ISessionService _sessionService;

    public AuthenticatedAuthorize(
        ILoggerService logger,
        IHaloServiceFactory haloServiceFactory
    ) {
        _logger = logger;
        _sessionService = haloServiceFactory.GetService<SessionService>(Enums.ServiceType.AppService) ?? throw new HaloArgumentNullException<AuthenticatedAuthorize>(nameof(SessionService));
    }

    public void OnAuthorization(AuthorizationFilterContext context) {
        _logger.Log(new LoggerBinding<AuthenticatedAuthorize> { Location = nameof(OnAuthorization) });
        var requestHeaders = context.HttpContext.Request.Headers;
        var destination = context.HttpContext.Request.Path.Value!;
        
        var authorization = _sessionService.Get<Authorization>(
            Equals(destination, "/authentication/verify-otp")
                ? Enums.SessionKey.PreAuthorization.GetValue()!
                : Enums.SessionKey.Authorization.GetValue()!
        );

        var (_, accountIdFromRequest) = requestHeaders.Single(x => x.Key.ToLower().Equals(nameof(HttpHeaderKeys.AccountId).ToLower()));
        if (!Equals(authorization!.AccountId, accountIdFromRequest.ToString())) {
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.InvalidUser.GetValue()}");
            return;
        }

        if (!authorization.IsPreAuthorization) {
            var (_, bearerTokenHeader) = requestHeaders.Single(x => x.Key.ToLower().Equals(nameof(HttpHeaderKeys.Authorization).ToLower()));
            var clientBearerToken = bearerTokenHeader.ToString().Split(Constants.MonoSpace).Last();
            
            if (!Equals(authorization.BearerToken, clientBearerToken)) {
                context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.MismatchedBearerToken.GetValue()}");
                return;
            }
        }
        
        var (_, clientAuthorizationToken) = requestHeaders.Single(x => x.Key.ToLower().Equals(nameof(HttpHeaderKeys.AccessToken).ToLower()));
        if (!Equals(authorization.AuthorizationToken, clientAuthorizationToken.ToString())) {
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.MismatchedAccessToken.GetValue()}");
            return;
        }
            
        // if (authorization.AuthorizedTimestamp + authorization.ValidityDuration < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        //     context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.AuthorizationExpired.GetValue()}");

        if (!authorization.IsPreAuthorization) return;
        
        if (!context.HttpContext.Request.Path.HasValue) {
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.PreAuthorizeNoPath.GetValue()}");
            return;
        }
        
        if (!Equals(destination, "/authentication/verify-otp"))
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.PreAuthorizeWrongPath.GetValue()}");
    }
}
