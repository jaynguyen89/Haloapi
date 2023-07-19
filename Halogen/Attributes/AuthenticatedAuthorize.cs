using System.Net;
using Halogen.Bindings.ViewModels;
using Halogen.Bindings.ServiceBindings;
using Halogen.FactoriesAndMiddlewares.Interfaces;
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
        _sessionService = haloServiceFactory.GetService<SessionService>(Enums.ServiceType.AppService)!;
    }

    public void OnAuthorization(AuthorizationFilterContext context) {
        _logger.Log(new LoggerBinding<AuthenticatedAuthorize> { Location = nameof(OnAuthorization) });
        var requestHeaders = context.HttpContext.Request.Headers;

        var authenticatedUser = _sessionService.Get<Authorization>(nameof(Authorization));
        var (_, accountIdFromRequest) = requestHeaders.Single(x => x.Key.ToLower().Equals(nameof(HttpHeaderKeys.AccountId).ToLower()));

        if (!Equals(authenticatedUser!.AccountId, accountIdFromRequest.ToString()))
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.InvalidUser.GetValue()}");

        var (_, bearerTokenHeader) = requestHeaders.Single(x => x.Key.ToLower().Equals(nameof(HttpHeaderKeys.Authorization).ToLower()));
        var clientBearerToken = bearerTokenHeader.ToString().Split(Constants.MonoSpace).Last();

        if (!Equals(authenticatedUser.BearerToken, clientBearerToken))
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.MismatchedBearerToken.GetValue()}");

        var (_, clientAuthorizationToken) = requestHeaders.Single(x => x.Key.ToLower().Equals(nameof(HttpHeaderKeys.AuthorizationToken).ToLower()));
        if (!Equals(authenticatedUser.AuthorizationToken, clientAuthorizationToken.ToString()))
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.MismatchedAuthToken.GetValue()}");
        
        if (authenticatedUser.AuthorizedTimestamp + authenticatedUser.ValidityDuration < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.AuthorizationExpired.GetValue()}");
    }
}