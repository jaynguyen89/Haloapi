using Microsoft.AspNetCore.Mvc.Filters;

namespace Halogen.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class CsrfFilter: ActionFilterAttribute {

    public CsrfFilter() { }

    public override void OnActionExecuting(ActionExecutingContext context) {
        base.OnActionExecuting(context);
        
        
    }

    public override void OnActionExecuted(ActionExecutedContext context) {
        base.OnActionExecuted(context);
        
        
    }
}