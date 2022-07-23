using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Halogen.Attributes; 

public class ValidRecaptchaAuthorize: AuthorizeAttribute, IAuthorizationFilter {

    public void OnAuthorization(AuthorizationFilterContext context) {
        
    }
}