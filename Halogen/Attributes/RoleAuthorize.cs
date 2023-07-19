using System.Net;
using Halogen.Bindings.ViewModels;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Authorization = Halogen.Bindings.ServiceBindings.Authorization;

namespace Halogen.Attributes; 

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RoleAuthorize: AuthorizeAttribute, IAuthorizationFilter {
    
    private readonly Enums.Role[] _authorizedRoles;

    public RoleAuthorize(params Enums.Role[] authorizedRoles) {
        _authorizedRoles = authorizedRoles;
    }

    public void OnAuthorization(AuthorizationFilterContext context) {
        var logger = context.HttpContext.RequestServices.GetService<ILoggerService>();
        logger?.Log(new LoggerBinding<RoleAuthorize> { Location = nameof(OnAuthorization) });

        var session = context.HttpContext.Session;
        var authenticatedUser = JsonConvert.DeserializeObject<Authorization>(session.GetString(nameof(Authorization)) ?? string.Empty);
        if (authenticatedUser is null)
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(RoleAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.InvalidUser.GetValue()}");
        
        if (!_authorizedRoles.Any(authenticatedUser!.Roles.Contains))
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(RoleAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.InvalidRole.GetValue()}");
    }
}