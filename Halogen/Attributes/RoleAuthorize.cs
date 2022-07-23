using Halogen.Bindings.ApiBindings;
using Halogen.Bindings.ServiceBindings;
using HelperLibrary;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace Halogen.Attributes; 

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class RoleAuthorize: AuthorizeAttribute, IAuthorizationFilter {
    
    private readonly Enums.Role[] _authorizedRoles;

    internal RoleAuthorize(params Enums.Role[] authorizedRoles) {
        _authorizedRoles = authorizedRoles;
    }

    public void OnAuthorization(AuthorizationFilterContext context) {
        var logger = context.HttpContext.RequestServices.GetService<ILoggerService>();
        logger?.Log(new LoggerBinding<RoleAuthorize> { Location = nameof(OnAuthorization) });

        var session = context.HttpContext.Session;
        var authenticatedUser = JsonConvert.DeserializeObject<Authorization>(session.GetString(nameof(Authorization)) ?? string.Empty);
        if (authenticatedUser is null)
            context.Result = new UnauthorizedObjectResult(new ClientResponse {
                Result = Enums.ApiResult.FAILED,
                Data = $"{nameof(RoleAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.INVALID_USER.GetEnumValue<string>()}"
            });
        
        if (!_authorizedRoles.Any(authenticatedUser!.Roles.Contains))
            context.Result = new UnauthorizedObjectResult(new ClientResponse {
                Result = Enums.ApiResult.FAILED,
                Data = $"{nameof(RoleAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.INVALID_ROLE.GetEnumValue<string>()}"
            });
    }
}