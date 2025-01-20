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
//[AutoValidateAntiforgeryToken]
[ServiceFilter(typeof(AuthenticatedAuthorize))]
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

    /// <summary>
    /// Get the most crucial information for the Authenticated User after login.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     GET /account/get-authenticated-user-info
    ///     Headers
    ///         AccountId: string
    ///         AccessToken: string
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="accountId">Mapped from header.</param>
    /// <response code="200">
    /// Successful request with data as follows:
    /// <code>
    /// {
    ///     accountId: string,
    ///     profileId: string,
    ///     username: string,
    ///     roles: Array:number
    ///     fullName: string,
    ///     emailAddress: string,
    ///     phoneNumber: {
    ///         regionCode: string,
    ///         phoneNumber: string,
    ///     }
    /// }
    /// </code>
    /// </response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
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

    /// <summary>
    /// Get the email information for the Authenticated User after login.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     GET /account/get-email-address-credential
    ///     Headers
    ///         AccountId: string
    ///         AccessToken: string
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="accountId">Mapped from header.</param>
    /// <response code="200">
    /// Successful request with data as follows:
    /// <code>
    /// {
    ///     emailAddress?: string,
    ///     isVerified?: boolean,
    /// }
    /// </code>
    /// </response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [HttpGet("get-email-address-credential")]
    public async Task<IActionResult> GetEmailAddressCredential([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<AccountController> { Location = nameof(GetEmailAddressCredential) });
        
        var emailAddressCredential = await _accountService.GetEmailAddressCredential(accountId);
        return emailAddressCredential is null
            ? new ErrorResponse()
            : new SuccessResponse(emailAddressCredential);
    }

    /// <summary>
    /// To request an Account Confirmation Email to confirm user's email address.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     PATCH /account/request-email-address-confirmation
    ///     Headers
    ///         AccountId: string
    ///         AccessToken: string
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="accountId">Mapped from header.</param>
    /// <response code="100">Continue - Email already confirmed.</response>
    /// <response code="200">Successful request.</response>
    /// <response code="205">ResetContent - The verification email was failed to send, all changes will be undone.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
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

    /// <summary>
    /// To change the user's current email address to a new one.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     PATCH /account/replace-email-address/{emailAddress}
    ///     Headers
    ///         AccountId: string
    ///         AccessToken: string
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="accountId">Mapped from header.</param>
    /// <param name="emailAddress">The new email address.</param>
    /// <response code="200">Successful request.</response>
    /// <response code="205">ResetContent - The verification email was failed to send, all changes will be undone.</response>
    /// <response code="400">BadRequest - The email address is not in a valid format.</response>
    /// <response code="409">Conflict - The email address is not unique.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [HttpPatch("replace-email-address/{emailAddress}")]
    [ServiceFilter(typeof(RecaptchaAuthorize))]
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

    /// <summary>
    /// To confirm the new email address.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     PATCH /account/confirm-email-address/{confirmationToken}
    ///     Headers
    ///         AccountId: string
    ///         AccessToken: string
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="accountId">Mapped from header.</param>
    /// <param name="confirmationToken">The token to confirm email address.</param>
    /// <response code="200">Successful request.</response>
    /// <response code="401">Gone - The token timestamp expired.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [HttpPatch("confirm-email-address/{confirmationToken}")]
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    public async Task<IActionResult> ConfirmEmailAddress([FromHeader] string accountId, [FromRoute] string confirmationToken) {
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