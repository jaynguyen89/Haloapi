using System.Net;
using System.Net.Mail;
using AssistantLibrary;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Services;
using Halogen.Attributes;
using Halogen.Bindings;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings.ApiBindings;
using Halogen.Bindings.ServiceBindings;
using Halogen.Bindings.ViewModels;
using Halogen.DbModels;
using Halogen.Services.AppServices.Interfaces;
using Halogen.Services.AppServices.Services;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Halogen.Controllers; 

[ApiController]
[Route("account")]
[AutoValidateAntiforgeryToken]
[ServiceFilter(typeof(AuthenticatedAuthorize))]
[ServiceFilter(typeof(RecaptchaAuthorize))]
[ServiceFilter(typeof(TwoFactorAuthorize))]
public sealed class AccountController: AppController {
    
    private readonly IContextService _contextService;
    private readonly IAccountService _accountService;
    private readonly ICacheService _cacheService;
    private readonly IMailService _mailService;

    public AccountController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        IHaloServiceFactory haloServiceFactory,
        IHaloConfigProvider haloConfigProvider,
        IAssistantServiceFactory assistantServiceFactory
    ) : base(ecosystem, logger, configuration, haloConfigProvider.GetHalogenConfigs()) {
        _contextService = haloServiceFactory.GetService<ContextService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AccountController>(nameof(ContextService));
        _accountService = haloServiceFactory.GetService<AccountService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AccountController>(nameof(AccountService));
        
        var cacheServiceFactory = haloServiceFactory.GetService<CacheServiceFactory>(Enums.ServiceType.AppService) ?? throw new HaloArgumentNullException<AccountController>(nameof(CacheServiceFactory));
        _cacheService = cacheServiceFactory.GetActiveCacheService();
        
        _mailService = assistantServiceFactory.GetService<MailService>() ?? throw new HaloArgumentNullException<AuthenticationController>(nameof(MailService));
    }
    
    [HttpGet("get-authenticated-user-info")]
    public async Task<IActionResult> GetAuthenticatedUserInfo([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<AccountController> { Location = nameof(GetAuthenticatedUserInfo) });

        var authenticatedUser = await _cacheService.GetCacheEntry<AuthenticatedUser>($"{nameof(AuthenticatedUser)}{Constants.Hyphen}{accountId}");
        if (authenticatedUser is not null) return new SuccessResponse(authenticatedUser);
        
        authenticatedUser = await _accountService.GetInformationForAuthenticatedUser(accountId);
        if (authenticatedUser is null) return new ErrorResponse();

        await _cacheService.InsertCacheEntry(new MemoryCacheEntry {
            Key = $"{nameof(AuthenticatedUser)}{Constants.Hyphen}{accountId}",
            Value = authenticatedUser,
            Priority = CacheItemPriority.High,
        });
        return new SuccessResponse(authenticatedUser);
    }

    [HttpGet("get-email-address-credential")]
    public async Task<IActionResult> GetEmailAddressCredential([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<AccountController> { Location = nameof(GetEmailAddressCredential) });
        
        var emailAddressCredential = await _accountService.GetEmailAddressCredential(accountId);
        return emailAddressCredential is null
            ? new ErrorResponse()
            : new SuccessResponse(emailAddressCredential);
    }

    [HttpPatch("request-email-address-confirmation")]
    public async Task<IActionResult> RequestEmailAddressConfirmation([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<AccountController> { Location = nameof(RequestEmailAddressConfirmation) });

        var account = await _accountService.GetAccountById(accountId);
        if (account is null) return new ErrorResponse();
        if (account.EmailConfirmed) return new ErrorResponse(HttpStatusCode.Continue);
        if (account.EmailAddressToken.IsString())
            return account.EmailAddressTokenTimestamp!.Value.Compute(_haloConfigs.EmailTokenValidityDuration, _haloConfigs.EmailTokenValidityDurationUnit) < DateTime.UtcNow
                ? new ErrorResponse(HttpStatusCode.NotAcceptable)
                : new ErrorResponse(HttpStatusCode.Gone);

        return await UpdateEmailAddressConfirmation(account);
    }

    [HttpPatch("replace-email-address/{emailAddress}")]
    public async Task<IActionResult> ReplaceEmailAddress([FromHeader] string accountId, [FromRoute] string emailAddress) {
        _logger.Log(new LoggerBinding<AccountController> { Location = nameof(ReplaceEmailAddress) });
        
        var account = await _accountService.GetAccountById(accountId);
        if (account is null) return new ErrorResponse();

        var loginInformation = new LoginInformation {
            EmailAddress = emailAddress,
        };
        var errors = await loginInformation.VerifyLoginInformation();
        if (errors.Count != 0) return new ErrorResponse(HttpStatusCode.BadRequest, errors);

        var isEmailAvailable = await _accountService.IsEmailAddressAvailableForNewAccount(emailAddress);
        if (!isEmailAvailable.HasValue) return new ErrorResponse();
        if (!isEmailAvailable.Value) return new ErrorResponse(HttpStatusCode.Conflict);

        return await UpdateEmailAddressConfirmation(account, emailAddress);
    }

    [HttpPatch("confirm-email-address")]
    public async Task<IActionResult> ConfirmEmailAddress([FromHeader] string accountId, [FromHeader] string confirmationToken) {
        _logger.Log(new LoggerBinding<AccountController> { Location = nameof(ConfirmEmailAddress) });
        
        var account = await _accountService.GetAccountById(accountId);
        if (account is null) return new ErrorResponse();
        
        if (account.EmailConfirmed) return new ErrorResponse(HttpStatusCode.Continue);
        if (!Equals(confirmationToken, account.EmailAddressToken)) return new ErrorResponse(HttpStatusCode.Forbidden);
        if (account.EmailAddressTokenTimestamp!.Value.Compute(_haloConfigs.EmailTokenValidityDuration, _haloConfigs.EmailTokenValidityDurationUnit) > DateTime.UtcNow)
            return new ErrorResponse(HttpStatusCode.Gone);

        account.EmailAddressToken = null;
        account.EmailAddressTokenTimestamp = null;
        account.EmailConfirmed = true;
        
        var accountUpdated = await _accountService.UpdateAccount(account);
        return !accountUpdated.HasValue || !accountUpdated.Value ? new ErrorResponse() : new SuccessResponse();
    }

    private async Task<IActionResult> UpdateEmailAddressConfirmation(Account account, string? emailAddress = null) {
        _logger.Log(new LoggerBinding<AccountController> { IsPrivate = true, Location = nameof(UpdateEmailAddressConfirmation) });

        if (emailAddress.IsString()) {
            account.EmailAddress = emailAddress;
            account.EmailConfirmed = false;
        }
        
        account.EmailAddressToken = StringHelpers.GenerateRandomString(
            NumberHelpers.GetRandomNumberInRangeInclusive(_haloConfigs.EmailTokenMinLength, _haloConfigs.EmailTokenMaxLength)
        );
        account.EmailAddressTokenTimestamp = DateTime.UtcNow;

        await _contextService.StartTransaction();
        var accountUpdated = await _accountService.UpdateAccount(account);
        if (!accountUpdated.HasValue || !accountUpdated.Value) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        var mailBinding = new MailBinding {
            ToReceivers = [new Recipient { EmailAddress = account.EmailAddress! }],
            Title = $"{Constants.ProjectName}: Confirm your email address",
            Priority = MailPriority.High,
            TemplateName = Enums.EmailTemplate.EmailAddressConfirmationEmail,
            Placeholders = new Dictionary<string, string> {
                { "USER_USERNAME", account.Username! },
                { "VERIFICATION_TOKEN", account.EmailAddressToken! },
                { "VALIDITY_DURATION", $"{_haloConfigs.PhoneTokenValidityDuration} {_haloConfigs.PhoneTokenValidityDurationUnit}s" },
            },
        };

        var isEmailSent = await _mailService.SendSingleEmail(mailBinding);
        if (!isEmailSent) {
            await _contextService.RevertTransaction();
            return new ErrorResponse(HttpStatusCode.ResetContent);
        }

        await _contextService.ConfirmTransaction();
        return new SuccessResponse();
    }
}