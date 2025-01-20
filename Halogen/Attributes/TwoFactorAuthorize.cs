using System.Net;
using Halogen.Bindings;
using Halogen.Bindings.ViewModels;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Authorization = Halogen.Bindings.ServiceBindings.Authorization;

namespace Halogen.Attributes; 

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class TwoFactorAuthorize: AuthorizeAttribute, IAuthorizationFilter {

    private readonly bool _twoFactorEnabled;
    private readonly ILoggerService _logger;
    
    public TwoFactorAuthorize(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ) {
        _logger = logger;
        var environment = ecosystem.GetEnvironment();
        
        _twoFactorEnabled = bool.Parse(configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.ServiceSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.ServiceSettings.TwoFactorEnabled)}")!);
    }

    public void OnAuthorization(AuthorizationFilterContext context) {
        _logger.Log(new LoggerBinding<TwoFactorAuthorize> { Location = nameof(OnAuthorization) });
        if (!_twoFactorEnabled) return;
        
        var authorization = JsonConvert.DeserializeObject<Authorization>(context.HttpContext.Session.GetString(Enums.SessionKey.Authorization.GetValue()!)!);
        if (authorization is null) {
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(TwoFactorAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.InternalServerError.GetValue()}");
            return;
        }
        
        if (!authorization.TwoFactorConfirmed.HasValue) return;
        if (!authorization.TwoFactorConfirmed.Value)
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(TwoFactorAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.NoTwoFactorToken.GetValue()}");
    }
}