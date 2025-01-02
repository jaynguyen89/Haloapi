﻿using System.Net;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings;
using Halogen.Bindings.ServiceBindings;
using Halogen.Bindings.ViewModels;
using Halogen.Controllers;
using Halogen.Services.AppServices.Interfaces;
using Halogen.Services.AppServices.Services;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Authorization = Halogen.Bindings.ServiceBindings.Authorization;

namespace Halogen.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AccountAndProfileAssociatedAuthorize: AuthorizeAttribute, IAuthorizationFilter {
    
    private readonly ILoggerService _logger;
    private readonly ISessionService _sessionService;
    private readonly IProfileService _profileService;

    public AccountAndProfileAssociatedAuthorize(
        ILoggerService logger,
        IHaloServiceFactory haloServiceFactory
    ) {
        _logger = logger;
        _sessionService = haloServiceFactory.GetService<SessionService>(Enums.ServiceType.AppService) ?? throw new HaloArgumentNullException<AuthenticatedAuthorize>(nameof(SessionService));
        _profileService = haloServiceFactory.GetService<ProfileService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<ProfileController>(nameof(ProfileService));
    }

    public void OnAuthorization(AuthorizationFilterContext context) {
        _logger.Log(new LoggerBinding<AccountAndProfileAssociatedAuthorize> { Location = nameof(OnAuthorization) });
        
        var authorization = _sessionService.Get<Authorization>(Enums.SessionKey.Authorization.GetValue()!);
        if (authorization is null) {
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.InternalServerError.GetValue()}");
            return;
        }

        var (_, profileIdFromRequest) = context.HttpContext.Request.Headers.Single(x => x.Key.ToLower().Equals(nameof(HttpHeaderKeys.ProfileId).ToLower()));
        var belongToAccount = _profileService.IsProfileIdBelongedToAccount(profileIdFromRequest!, authorization.AccountId).Result;
        if (!belongToAccount.HasValue) {
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.InternalServerError.GetValue()}");
            return;
        }
        
        if (!belongToAccount.Value)
            context.Result = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.AccountProfileUnassociated.GetValue()}");
    }
}