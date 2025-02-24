﻿using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using AssistantLibrary;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Interfaces.IServiceFactory;
using AssistantLibrary.Services;
using Halogen.Attributes;
using Halogen.Bindings.ApiBindings;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;
using Halogen.Bindings;
using Halogen.Bindings.ViewModels;
using Halogen.DbModels;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Services.AppServices.Interfaces;
using Halogen.Services.AppServices.Services;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using HelperLibrary.Shared.Helpers;
using Newtonsoft.Json;
using Authorization = Halogen.Bindings.ServiceBindings.Authorization;

namespace Halogen.Controllers; 

[ApiController]
[Route("authentication")]
//[AutoValidateAntiforgeryToken]
public sealed class AuthenticationController: AppController {

    private readonly IContextService _contextService;
    private readonly IAuthenticationService _authenticationService;
    private readonly ICryptoService _cryptoService;
    private readonly IMailService _mailService;
    private readonly IAccountService _accountService;
    private readonly IProfileService _profileService;
    private readonly IRoleService _roleService;
    private readonly IPreferenceService _preferenceService;
    private readonly ITrustedDeviceService _trustedDeviceService;
    private readonly IJwtService _jwtService;
    
    private readonly ISmsService _clickatellSmsHttpService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly RegionalizedPhoneNumberHandler _phoneNumberHandler;

    public AuthenticationController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        IHaloServiceFactory haloServiceFactory,
        IAssistantServiceFactory assistantServiceFactory,
        IHaloConfigProvider haloConfigProvider,
        RegionalizedPhoneNumberHandler phoneNumberHandler
    ) : base(ecosystem, logger, configuration, haloConfigProvider.GetHalogenConfigs()) {
        _contextService = haloServiceFactory.GetService<ContextService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AuthenticationController>(nameof(ContextService));
        _authenticationService = haloServiceFactory.GetService<AuthenticationService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AuthenticationController>(nameof(AuthenticationService));
        _cryptoService = assistantServiceFactory.GetService<CryptoService>() ?? throw new HaloArgumentNullException<AuthenticationController>(nameof(CryptoService));
        _mailService = assistantServiceFactory.GetService<MailService>() ?? throw new HaloArgumentNullException<AuthenticationController>(nameof(MailService));
        _accountService = haloServiceFactory.GetService<AccountService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AuthenticationController>(nameof(AccountService));
        _profileService = haloServiceFactory.GetService<ProfileService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AuthenticationController>(nameof(ProfileService));
        _roleService = haloServiceFactory.GetService<RoleService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AuthenticationController>(nameof(RoleService));
        _preferenceService = haloServiceFactory.GetService<PreferenceService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AuthenticationController>(nameof(PreferenceService));
        _trustedDeviceService = haloServiceFactory.GetService<TrustedDeviceService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<AuthenticationController>(nameof(TrustedDeviceService));
        _jwtService = haloServiceFactory.GetService<JwtService>(Enums.ServiceType.AppService) ?? throw new HaloArgumentNullException<AuthenticationController>(nameof(JwtService));
        
        _clickatellSmsHttpService = assistantServiceFactory.GetService<SmsServiceFactory>()?.GetActiveSmsService() ?? throw new HaloArgumentNullException<AuthenticationController>(nameof(SmsServiceFactory));
        _twoFactorService = assistantServiceFactory.GetService<TwoFactorService>() ?? throw new HaloArgumentNullException<AuthenticationController>(nameof(TwoFactorService));

        _phoneNumberHandler = phoneNumberHandler;
    }

    /// <summary>
    /// For guest. To check if the Email Address is unique in Halogen database.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     GET /check-email-availability/{emailAddress}
    ///     Headers
    ///         RecaptchaToken: string
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="emailAddress">The Email Address to be checked.</param>
    /// <response code="200" data="isEmailAvailable:boolean">Successful request.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpGet("check-email-availability/{emailAddress}")]
    public async Task<IActionResult> IsEmailAddressAvailable([FromRoute] string emailAddress) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(IsEmailAddressAvailable) });

        if (!emailAddress.IsString()) return new ErrorResponse(HttpStatusCode.BadRequest);

        var isEmailAvailable = await _accountService.IsEmailAddressAvailableForNewAccount(emailAddress);
        return !isEmailAvailable.HasValue
            ? new ErrorResponse()
            : new SuccessResponse(new { isEmailAvailable = isEmailAvailable.Value });
    }

    /// <summary>
    /// For guest. To check if the Phone Number is unique in Halogen database.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     GET /check-phone-number-availability
    ///     Headers
    ///         RecaptchaToken: string
    ///     Body
    ///         {
    ///             regionCode: string,
    ///             phoneNumber: string,
    ///         }
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="phoneNumber">The Phone Number to be checked.</param>
    /// <response code="200" data="isPhoneNumberAvailable:boolean">Successful request.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpGet("check-phone-number-availability")]
    public async Task<IActionResult> IsPhoneNumberAvailable([FromRoute] RegionalizedPhoneNumber phoneNumber) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(IsPhoneNumberAvailable) });

        var errors = await _phoneNumberHandler.VerifyPhoneNumberData(phoneNumber);
        if (errors.Length != 0) return new ErrorResponse(HttpStatusCode.BadRequest, errors);

        var isPhoneNumberAvailable = await _profileService.IsPhoneNumberAvailableForNewAccount(phoneNumber.Simplify());
        return !isPhoneNumberAvailable.HasValue
            ? new ErrorResponse()
            : new SuccessResponse(new { isPhoneNumberAvailable = isPhoneNumberAvailable.Value });
    }

    /// <summary>
    /// For guest. To register new Account using Email Address or Phone Number. The Email Address or Phone Number must be unique.
    /// The <c>role</c> will be <c>Enums.Role.Customer</c> by default. The Profile and Preference will be created with default database values.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     POST /authentication/register-account
    ///     Body
    ///         {
    ///             emailAddress?: string,
    ///             phoneNumber?: {
    ///                 regionCode: string,
    ///                 phoneNumber: string,
    ///             },
    ///             password: string,
    ///             passwordConfirm: string,
    ///             username: string,
    ///             profileData?: {
    ///                 gender?: number, // expects an enum value, see Enums.Genders
    ///                 givenName?: string,
    ///                 middleName?: string,
    ///                 familyName?: string,
    ///                 fullName?: string,
    ///             },
    ///         }
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="registrationData">The data required for creating new Account. See `RegistrationData`.</param>
    /// <response code="201">Successful request.</response>
    /// <response code="400">Bad request - Errors in `RegistrationData`.</response>
    /// <response code="409">Conflict - The Email Address or Phone Number is not unique.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPost("register-account")]
    public async Task<IActionResult> RegisterAccount([FromBody] RegistrationData registrationData) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(RegisterAccount) });
        
        var errors = await registrationData.VerifyRegistrationData(_phoneNumberHandler);
        if (errors.Count != 0) return new ErrorResponse(HttpStatusCode.BadRequest, errors);

        var registerByEmailAddress = registrationData.EmailAddress.IsString();
        if (registerByEmailAddress) {
            var isEmailAvailable = await _accountService.IsEmailAddressAvailableForNewAccount(registrationData.EmailAddress!);
            if (!isEmailAvailable.HasValue) return new ErrorResponse();
            if (!isEmailAvailable.Value) return new ErrorResponse(HttpStatusCode.Conflict, new { isEmailAvailable });
        }
        else {
            var isPhoneNumberAvailable = await _profileService.IsPhoneNumberAvailableForNewAccount(registrationData.PhoneNumber!.ToString());
            if (!isPhoneNumberAvailable.HasValue) return new ErrorResponse();
            if (!isPhoneNumberAvailable.Value) return new ErrorResponse(HttpStatusCode.Conflict, new { isPhoneNumberAvailable });
        }

        var (verificationTokenLength, saltLength) = GetTokenLengthsForNewAccount();
        var (hashedPassword, salt) = _cryptoService.GenerateHashAndSalt(registrationData.Password, saltLength);

        var newAccount = Account.CreateNewAccount(_useLongerId, registrationData.EmailAddress, salt, hashedPassword, verificationTokenLength, registrationData.Username);
        await _contextService.StartTransaction();

        var accountId = await _authenticationService.InsertNewAccount(newAccount);
        if (accountId is null) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }
        
        var newProfile = Profile.CreateNewProfile(_useLongerId, accountId, registerByEmailAddress, _haloConfigs.PhoneTokenMinLength, _haloConfigs.PhoneTokenMaxLength, registrationData.PhoneNumber, registrationData.ProfileData);
        
        var profileId = await _profileService.InsertNewProfile(newProfile);
        if (profileId is null) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        var role = await _roleService.GetRoleByName(Enums.Role.Customer.GetValue()!);
        if (role is null) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        var accountRoleId = await _roleService.InsertNewAccountRole(new AccountRole {
            Id = StringHelpers.NewGuid(_useLongerId),
            AccountId = accountId,
            RoleId = role.Id,
            IsEffective = true
        });
        if (accountRoleId is null) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        var defaultPreference = Preference.CreatePreferenceForNewAccount(_useLongerId, accountId, registerByEmailAddress);
        var preferenceId = await _preferenceService.InsertNewPreference(defaultPreference);
        if (preferenceId is null) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        if (registerByEmailAddress) {
            var accountActivationEmailExpiry = newAccount.EmailAddressTokenTimestamp!.Value
                                                         .Compute(_haloConfigs.EmailTokenValidityDuration, _haloConfigs.EmailTokenValidityDurationUnit)
                                                         .Format(Enums.DateFormat.DDMMYYYYS, Enums.TimeFormat.HHMMTTC);
            
            var mailBinding = new MailBinding {
                ToReceivers = [new Recipient { EmailAddress = newAccount.EmailAddress! }],
                Title = $"{Constants.ProjectName}: Activate your account",
                Priority = MailPriority.High,
                TemplateName = Enums.EmailTemplate.AccountActivationEmail,
                Placeholders = new Dictionary<string, string> {
                    { "ACCOUNT_ID", accountId },
                    { "EMAIL_ADDRESS", newAccount.EmailAddress! },
                    { "REGISTRATION_TOKEN", Uri.EscapeDataString(newAccount.EmailAddressToken!) },
                    { "USER_NAME", newAccount.Username is null ? string.Empty : Uri.EscapeDataString(newAccount.Username)},
                    { "VALID_UNTIL_DATETIME", accountActivationEmailExpiry },
                },
            };

            var isAccountActivationEmailSent = await _mailService.SendSingleEmail(mailBinding);
            if (!isAccountActivationEmailSent) {
                await _contextService.RevertTransaction();
                return new ErrorResponse();
            }
        }
        else {
            var smsContent = _haloConfigs.AccountActivationSmsContent
                             .Replace("ACCOUNT_ID", accountId)
                             .Replace("CLIENT_BASE_URI", _haloConfigs.ClientBaseUri)
                             .Replace("PHONE_NUMBER", registrationData.PhoneNumber!.Simplify())
                             .Replace("REGISTRATION_TOKEN", Uri.EscapeDataString(newProfile.PhoneNumberToken!))
                             .Replace("USER_NAME", newAccount.Username is null ? string.Empty : Uri.EscapeDataString(newAccount.Username))
                             .Replace("VALIDITY_DURATION", $"{_haloConfigs.PhoneTokenValidityDuration} {_haloConfigs.PhoneTokenValidityDurationUnit}s");
            var smsBinding = new SingleSmsBinding {
                SmsContent = smsContent,
                Receivers = [registrationData.PhoneNumber!.ToString()],
            };

            var smsResult = await _clickatellSmsHttpService.SendSingleSms(smsBinding);
            if (smsResult is null || smsResult.Length != 0) {
                await _contextService.RevertTransaction();
                return new ErrorResponse();
            }
        }

        await _contextService.ConfirmTransaction();
        return new SuccessResponse(HttpStatusCode.Created);
    }

    /// <summary>
    /// For guest. To send a Secret Code to Email Address or Phone Number, whichever is the main credential set by user if they have both.
    /// The Secret Code will be sent <b>only</b> if the Account or Profile has an <u>active</u> Token for Account validation.
    /// This endpoint is configurable on or off and protected by Google Recaptcha.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     GET /send-secret-code?destination={number}
    ///     Headers
    ///         AccountId: string
    ///         RecaptchaToken: string
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="accountId">ID of the Account to receive the Secret Code.</param>
    /// <param name="destination">An enum value indicating whether the Secret Code should be sent to Email Address or Phone Number.</param>
    /// <response code="200">Successful request.</response>
    /// <response code="100">Continue - This endpoint is disabled. Not necessary to send and verify the Secret Code. Client may continue its next frontend process.</response>
    /// <response code="410">Gone - The Token for Account validation is existed but has expired.</response>
    /// <response code="404">Not Found - The Token for Account validation is not existed.</response>
    /// <response code="422">Unprocessable Entity - The Account has no information for the destination.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpGet("send-secret-code")]
    public async Task<IActionResult> SendSecretCode([FromHeader] string accountId, [FromQuery] Enums.TokenDestination destination) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(SendSecretCode) });

        if (!_haloConfigs.EnableSecretCode) return new ErrorResponse(HttpStatusCode.Continue);
        
        var account = await _accountService.GetAccountById(accountId);
        if (account is null) return new ErrorResponse();

        var profile = await _profileService.GetProfileByAccountId(accountId);
        if (profile is null) return new ErrorResponse();

        if (destination == Enums.TokenDestination.Sms) {
            if (!profile.PhoneNumber.IsString()) return new ErrorResponse(HttpStatusCode.UnprocessableEntity);
            if (!profile.PhoneNumberToken.IsString() || !profile.PhoneNumberTokenTimestamp.HasValue) return new ErrorResponse(HttpStatusCode.NotFound);
            if (profile.PhoneNumberTokenTimestamp.Value > DateTime.UtcNow) return new ErrorResponse(HttpStatusCode.Gone);
        }
        else {
            if (!account.EmailAddress.IsString()) return new ErrorResponse(HttpStatusCode.UnprocessableEntity);
            if (!account.EmailAddressToken.IsString() || !account.EmailAddressTokenTimestamp.HasValue) return new ErrorResponse(HttpStatusCode.NotFound);
            if (account.EmailAddressTokenTimestamp.Value > DateTime.UtcNow) return new ErrorResponse(HttpStatusCode.Gone);
        }

        account.SecretCode = StringHelpers.GenerateRandomString(Constants.SecretCodeLength, false, false);
        account.SecretCodeTimestamp = DateTime.UtcNow;

        await _contextService.StartTransaction();
        var accountUpdateResult = await _accountService.UpdateAccount(account);
        if (!accountUpdateResult.HasValue) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        if (destination == Enums.TokenDestination.Sms) {
            var smsContent = _haloConfigs.SecretCodeSmsContent
                .Replace("SECRET_CODE", account.SecretCode)
                .Replace("VALIDITY_DURATION", $"{_haloConfigs.SecretCodeValidityDuration} {_haloConfigs.SecretCodeValidityDurationUnit}s");

            var regionalizedPhoneNumber = RegionalizedPhoneNumber.Deserialize(profile.PhoneNumber!);
            if (regionalizedPhoneNumber is null) return new ErrorResponse();
            
            var smsBinding = new SingleSmsBinding {
                SmsContent = smsContent,
                Receivers = [regionalizedPhoneNumber.ToPhoneNumber()],
            };
            
            var smsResult = await _clickatellSmsHttpService.SendSingleSms(smsBinding);
            if (smsResult is null || smsResult.Length != 0) {
                await _contextService.RevertTransaction();
                return new ErrorResponse();
            }
        }
        else {
            var mailBinding = new MailBinding {
                ToReceivers = [new Recipient { EmailAddress = account.EmailAddress! }],
                Title = $"{Constants.ProjectName}: Secret Code",
                Priority = MailPriority.High,
                TemplateName = Enums.EmailTemplate.SecretCodeEmail,
                Placeholders = new Dictionary<string, string> {
                    { "EMAIL_ADDRESS", account.EmailAddress! },
                    { "SECRET_CODE", account.SecretCode },
                    { "VALIDITY_DURATION", $"{_haloConfigs.SecretCodeValidityDuration} {_haloConfigs.SecretCodeValidityDurationUnit}s" },
                },
            };

            var isSecretCodeEmailSent = await _mailService.SendSingleEmail(mailBinding);
            if (!isSecretCodeEmailSent) {
                await _contextService.RevertTransaction();
                return new ErrorResponse();
            }
        }

        await _contextService.ConfirmTransaction();
        return new SuccessResponse();
    }

    /// <summary>
    /// For guest. To forward the Token for Account validation to the other credential, which is Phone Number or Email Address if they don't have access to the current credential.
    /// Only the Tokens for Account Recovery and One-Time Password can be forwarded. The other Tokens should be renewed, meaning it is bounded to the credential.
    /// The Token can <b>only</b> be forwarded if it is existed <b>and</b> <u>not</u> expired.
    /// This endpoint is protected by Google Recaptcha and Secret Code.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     GET /forward-token
    ///     Headers
    ///         AccountId: string
    ///         RecaptchaToken: string
    ///     Body
    ///         {
    ///             destination: number, // expects an enum value, see Enums.TokenDestination
    ///             isOtp: boolean,
    ///             secretCode?: string,
    ///         }
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="accountId">ID of the Account to forward the Token.</param>
    /// <param name="tokenData">The data required to determine the API behaviour.</param>
    /// <response code="200">Successful request.</response>
    /// <response code="400">Bad Request - The Secret Code is missing in Request Body.</response>
    /// <response code="403">Forbidden - The Secret Code from client does not pass API validation.</response>
    /// <response code="410">Gone - The Secret Code has expired.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    /// <response code="501">Not Implemented - The SMS or Email services was unable to send message to `destination`.</response>
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpGet("forward-token")]
    public async Task<IActionResult> ForwardToken([FromHeader] string accountId, [FromBody] TokenData tokenData) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(ForwardToken) });

        var account = await _accountService.GetAccountById(accountId);
        if (account is null) return new ErrorResponse();

        if (_haloConfigs.EnableSecretCode) {
            if (!tokenData.SecretCode.IsString()) return new ErrorResponse(HttpStatusCode.BadRequest);

            var secretCodeValidation = IsSecretCodeValid(tokenData.SecretCode!, account);
            
            if (!secretCodeValidation.HasValue) return new ErrorResponse(HttpStatusCode.Forbidden);
            if (!secretCodeValidation.Value) return new ErrorResponse(HttpStatusCode.Gone);
        }
        
        var profile = await _profileService.GetProfileByAccountId(accountId);
        if (profile is null) return new ErrorResponse();

        var (token, tokenTimestamp) = tokenData.IsOtp
            ? new Tuple<string?, DateTime>(account.OneTimePassword, account.OneTimePasswordTimestamp!.Value)
            : new Tuple<string?, DateTime>(account.RecoveryToken, account.RecoveryTokenTimestamp!.Value);
        
        // Only tokens for Account Recovery and One Time Password can be forwarded, other tokens should be renewed
        DateTime? tokenElapsingTime = tokenData.IsOtp
            ? tokenTimestamp.Compute(_haloConfigs.OtpValidityDuration, _haloConfigs.OtpValidityDurationUnit)
            : tokenTimestamp.Compute(_haloConfigs.RecoveryTokenValidityDuration, _haloConfigs.RecoveryTokenValidityDurationUnit);
        
        if (tokenElapsingTime > DateTime.UtcNow) return new ErrorResponse(HttpStatusCode.Gone);

        var tokenSendingExpression = tokenData.Destination switch {
            Enums.TokenDestination.Email => (Func<Task<bool?>>)(async () => {
                var emailExpiryTimestamp = tokenElapsingTime.Value.Format(Enums.DateFormat.DDMMYYYYS, Enums.TimeFormat.HHMMTTC);

                var mailBinding = tokenData.IsOtp
                    ? new MailBinding {
                        ToReceivers = [new Recipient { EmailAddress = account.EmailAddress! }],
                        Title = $"{Constants.ProjectName}: Your OTP",
                        Priority = MailPriority.High,
                        TemplateName = Enums.EmailTemplate.OneTimePasswordEmail,
                        Placeholders = new Dictionary<string, string> {
                            { "EMAIL_ADDRESS", account.EmailAddress! },
                            { "ONE_TIME_PASSWORD", Uri.EscapeDataString(token!) },
                            { "VALIDITY_DURATION", _haloConfigs.OtpValidityDuration.ToString() },
                            { "VALIDITY_UNIT", _haloConfigs.OtpValidityDurationUnit.GetValue()! },
                        },
                    }
                    : new MailBinding {
                        ToReceivers = [new Recipient { EmailAddress = account.EmailAddress! }],
                        Title = $"{Constants.ProjectName}: Recover your account",
                        Priority = MailPriority.High,
                        TemplateName = Enums.EmailTemplate.AccountRecoveryEmail,
                        Placeholders = new Dictionary<string, string> {
                            { "EMAIL_ADDRESS", account.EmailAddress! },
                            { "RECOVERY_TOKEN", Uri.EscapeDataString(token!) },
                            { "VALID_UNTIL_DATETIME", emailExpiryTimestamp },
                        },
                    };
                
                return await _mailService.SendSingleEmail(mailBinding);
            }),
            _ => async () => {
                var smsContent = tokenData.IsOtp
                    ? _haloConfigs.OneTimePasswordSmsContent
                        .Replace("ONE_TIME_PASSWORD", token)
                        .Replace("VALIDITY_DURATION", $"{_haloConfigs.OtpValidityDuration} {_haloConfigs.OtpValidityDurationUnit.GetValue()}")
                    : _haloConfigs.AccountRecoverySmsContent
                        .Replace("ACCOUNT_RECOVERY_TOKEN", token)
                        .Replace("VALIDITY_DURATION", $"{_haloConfigs.RecoveryTokenValidityDuration} {_haloConfigs.RecoveryTokenValidityDurationUnit}");
                
                if (!profile.PhoneNumber.IsString()) return null;
                
                var smsBinding = new SingleSmsBinding {
                    SmsContent = smsContent,
                    Receivers = [profile.PhoneNumber!],
                };
                
                var smsResult = await _clickatellSmsHttpService.SendSingleSms(smsBinding);
                if (smsResult is null || smsResult.Length != 0) return null;
                return true;
            }
        };

        var result = tokenSendingExpression.Invoke().Result;
        
        if (!result.HasValue) return new ErrorResponse();
        return !result.Value ? new ErrorResponse(HttpStatusCode.NotImplemented) : new SuccessResponse();
    }

    /// <summary>
    /// For guest. To request (renew) their Account Activation Token when it was expired.
    /// The Token can <b>only</b> be renewed if it has been expired, otherwise, an error will respond.
    /// This endpoint is protected by Google Recaptcha and Secret Code.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     PUT /renew-token
    ///     Headers
    ///         AccountId: string
    ///         RecaptchaToken: string
    ///     Body
    ///         {
    ///             destination: number, // expects an enum value, see Enums.TokenDestination
    ///             secretCode?: string,
    ///             currentToken: string,
    ///         }
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="accountId">ID of the Account to renew the Token.</param>
    /// <param name="tokenData">The data required to determine the API behaviour.</param>
    /// <response code="200">Successful request.</response>
    /// <response code="400">Bad Request - The Secret Code is missing in Request Body.</response>
    /// <response code="403">Forbidden - The Secret Code from client does not pass API validation.</response>
    /// <response code="410">Gone - The Secret Code has expired.</response>
    /// <response code="409">Conflict - The current Token sent by client does not match the Account's Token.</response>
    /// <response code="416">Requested Range Not Satisfiable - The Token has not expired, so not renewable.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    /// <response code="501">Not Implemented - The SMS or Email services was unable to send message to `destination`.</response>
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPut("renew-token")]
    public async Task<IActionResult> RenewRegistrationToken([FromHeader] string accountId, [FromBody] TokenData tokenData) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(RenewRegistrationToken) });
        
        var account = await _accountService.GetAccountById(accountId);
        if (account is null) return new ErrorResponse();
        
        if (_haloConfigs.EnableSecretCode) {
            if (!tokenData.SecretCode.IsString()) return new ErrorResponse(HttpStatusCode.BadRequest);
            
            var secretCodeValidation = IsSecretCodeValid(tokenData.SecretCode!, account);
            
            if (!secretCodeValidation.HasValue) return new ErrorResponse(HttpStatusCode.Forbidden);
            if (!secretCodeValidation.Value) return new ErrorResponse(HttpStatusCode.Gone);
        }
        
        var profile = await _profileService.GetProfileByAccountId(accountId);
        if (profile is null) return new ErrorResponse();

        var (token, tokenTimestamp) = tokenData.Destination == Enums.TokenDestination.Email
            ? new Tuple<string, DateTime>(account.EmailAddressToken!, account.EmailAddressTokenTimestamp!.Value)
            : new Tuple<string, DateTime>(profile.PhoneNumberToken!, profile.PhoneNumberTokenTimestamp!.Value);
        
        if (!Equals(token, tokenData.CurrentToken)) return new ErrorResponse(HttpStatusCode.Conflict);

        var tokenElapsingTime = tokenData.Destination == Enums.TokenDestination.Email
            ? tokenTimestamp.Compute(_haloConfigs.EmailTokenValidityDuration, _haloConfigs.EmailTokenValidityDurationUnit)
            : tokenTimestamp.Compute(_haloConfigs.PhoneTokenValidityDuration, _haloConfigs.PhoneTokenValidityDurationUnit);
        
        if (tokenElapsingTime > DateTime.UtcNow) return new ErrorResponse(HttpStatusCode.RequestedRangeNotSatisfiable);

        token = tokenData.Destination == Enums.TokenDestination.Email
            ? StringHelpers.GenerateRandomString(GetTokenLengthsForNewAccount().Item1, true)
            : StringHelpers.GenerateRandomString(NumberHelpers.GetRandomNumberInRangeInclusive(_haloConfigs.PhoneTokenMinLength, _haloConfigs.PhoneTokenMaxLength), true);

        var newTokenTimestamp = DateTime.UtcNow;
        var updateTokenExpression = tokenData.Destination == Enums.TokenDestination.Email
            ? (Func<Task<bool?>>)(async () => {
                account.EmailAddressToken = token;
                account.EmailAddressTokenTimestamp = newTokenTimestamp;
                return await _accountService.UpdateAccount(account);
            })
            : async () => {
                profile.PhoneNumberToken = token;
                profile.PhoneNumberTokenTimestamp = newTokenTimestamp;
                return await _profileService.UpdateProfile(profile);
            };

        await _contextService.StartTransaction();
        var tokenUpdateResult = updateTokenExpression.Invoke().Result;
        if (!tokenUpdateResult.HasValue || !tokenUpdateResult.Value) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        var tokenSendingExpression = tokenData.Destination == Enums.TokenDestination.Email
            ? (Func<Task<bool?>>)(async () => {
                var newTokenExpiry = newTokenTimestamp.Compute(_haloConfigs.EmailTokenValidityDuration, _haloConfigs.EmailTokenValidityDurationUnit)
                    .Format(Enums.DateFormat.DDMMYYYYS, Enums.TimeFormat.HHMMTTC);

                var mailBinding = new MailBinding {
                    ToReceivers = [new Recipient { EmailAddress = account.EmailAddress! }],
                    Title = $"{Constants.ProjectName}: Activate your account",
                    Priority = MailPriority.High,
                    TemplateName = Enums.EmailTemplate.AccountActivationEmail,
                    Placeholders = new Dictionary<string, string> {
                        { "EMAIL_ADDRESS", account.EmailAddress! },
                        { "REGISTRATION_TOKEN", Uri.EscapeDataString(token) },
                        { "USER_NAME", account.Username is null ? string.Empty : Uri.EscapeDataString(account.Username) },
                        { "VALID_UNTIL_DATETIME", newTokenExpiry },
                    },
                };

                return await _mailService.SendSingleEmail(mailBinding);
            })
            : async () => {
                var smsContent = _haloConfigs.AccountActivationSmsContent
                    .Replace("CLIENT_BASE_URI", _haloConfigs.ClientBaseUri)
                    .Replace("PHONE_NUMBER", JsonConvert.DeserializeObject<RegionalizedPhoneNumber>(profile.PhoneNumber!)!.Simplify())
                    .Replace("REGISTRATION_TOKEN", Uri.EscapeDataString(profile.PhoneNumberToken!))
                    .Replace("USER_NAME", account.Username is null ? string.Empty : Uri.EscapeDataString(account.Username))
                    .Replace("VALIDITY_DURATION", $"{_haloConfigs.PhoneTokenValidityDuration} {_haloConfigs.PhoneTokenValidityDurationUnit}s");

                var phoneNumber = JsonConvert.DeserializeObject<RegionalizedPhoneNumber>(profile.PhoneNumber!);
                var smsBinding = new SingleSmsBinding {
                    SmsContent = smsContent,
                    Receivers = [phoneNumber!.ToString()],
                };

                var smsResult = await _clickatellSmsHttpService.SendSingleSms(smsBinding);
                return !smsResult?.Any();
            };

        var tokenSendingResult = tokenSendingExpression.Invoke().Result;
        if (!tokenSendingResult.HasValue) return new ErrorResponse();
        if (!tokenSendingResult.Value) return new ErrorResponse(HttpStatusCode.NotImplemented);

        await _contextService.ConfirmTransaction();
        return new SuccessResponse();
    }

    /// <summary>
    /// For guest. To activate their newly created account. This endpoint is protected by Google Recaptcha and Secret Code.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     PUT /activate-account
    ///     Headers
    ///         AccountId: string
    ///         RecaptchaToken: string
    ///     Body
    ///         {
    ///             destination: number, // expects an enum value, see Enums.TokenDestination
    ///             secretCode?: string,
    ///             currentToken: string,
    ///         }
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="accountId">ID of the Account to be activated.</param>
    /// <param name="tokenData">The data required for Account Activation.</param>
    /// <response code="200">Successful request.</response>
    /// <response code="400">Bad Request - The Secret Code is missing in Request Body.</response>
    /// <response code="403">Forbidden - The Secret Code from client does not pass API validation.</response>
    /// <response code="410" data="secret-code">Gone - The Secret Code has expired.</response>
    /// <response code="409">Conflict - The Activation Token sent by client does not match the Account's Token.</response>
    /// <response code="410" data="activation-token">The Activation Token has expired.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPut("activate-account")]
    public async Task<ActionResult> ActivateAccount([FromHeader] string accountId, [FromBody] TokenData tokenData) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(ActivateAccount) });
        
        var account = await _accountService.GetAccountById(accountId);
        if (account is null) return new ErrorResponse();
        
        if (_haloConfigs.EnableSecretCode) {
            if (!tokenData.SecretCode.IsString()) return new ErrorResponse(HttpStatusCode.BadRequest);
            
            var secretCodeValidation = IsSecretCodeValid(tokenData.SecretCode!, account);
            
            if (!secretCodeValidation.HasValue) return new ErrorResponse(HttpStatusCode.Forbidden);
            if (!secretCodeValidation.Value) return new ErrorResponse(HttpStatusCode.Gone, new { data = "secret-code" });
        }
        
        var profile = await _profileService.GetProfileByAccountId(accountId);
        if (profile is null) return new ErrorResponse();

        var activationExpression = tokenData.Destination == Enums.TokenDestination.Email
            ? (Func<Task<bool?>>)(async () => {
                if (!Equals(account.EmailAddressToken, tokenData.CurrentToken)) return null;

                var tokenElapsingTime = account.EmailAddressTokenTimestamp!.Value.Compute(_haloConfigs.EmailTokenValidityDuration, _haloConfigs.EmailTokenValidityDurationUnit);
                if (tokenElapsingTime < DateTime.UtcNow) return false;

                account.EmailAddressToken = null;
                account.EmailAddressTokenTimestamp = null;
                account.EmailConfirmed = true;
                account.SecretCode = default;
                account.SecretCodeTimestamp = default;

                return await _accountService.UpdateAccount(account);
            })
            : async () => {
                if (!Equals(profile.PhoneNumberToken, tokenData.CurrentToken)) return null;

                var tokenElapsingTime = profile.PhoneNumberTokenTimestamp!.Value.Compute(_haloConfigs.PhoneTokenValidityDuration, _haloConfigs.PhoneTokenValidityDurationUnit);
                if (tokenElapsingTime < DateTime.UtcNow) return false;

                profile.PhoneNumberToken = null;
                profile.PhoneNumberTokenTimestamp = null;
                profile.PhoneNumberConfirmed = true;
                account.SecretCode = default;
                account.SecretCodeTimestamp = default;

                var profileUpdateResult = await _profileService.UpdateProfile(profile);
                var accountUpdateResult = await _accountService.UpdateAccount(account);

                if (profileUpdateResult is null || accountUpdateResult is null)
                    return default;

                return profileUpdateResult.Value && accountUpdateResult.Value;
            };

        await _contextService.StartTransaction();
        var result = activationExpression.Invoke().Result;
        if (!result.HasValue) {
            await _contextService.RevertTransaction();
            return new ErrorResponse(HttpStatusCode.Conflict);
        }

        if (!result.Value) {
            await _contextService.RevertTransaction();
            return new ErrorResponse(HttpStatusCode.Gone, new { data = "activation-token" });
        }

        await _contextService.ConfirmTransaction();
        return new SuccessResponse();
    }

    /// <summary>
    /// For Customer. To perform login using Email Address or Phone Number, and Password.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     POST /authenticate-by-credentials
    ///     Body
    ///         {
    ///             emailAddress?: string,
    ///             phoneNumber?: {
    ///                 regionCode: string,
    ///                 phoneNumber: string,
    ///             },
    ///             password: string,
    ///             isTrusted: boolean,
    ///             deviceInformation?: {
    ///                 name?: string,
    ///                 deviceType?: string,
    ///                 uniqueIdentifier?: string,
    ///                 uniqueIdentifierType?: string,
    ///                 location?: string,
    ///                 ipAddress?: string,
    ///                 operatingSystem?: string,
    ///                 browserType?: string,
    ///             },
    ///         }
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="authenticationData">The data required for login.</param>
    /// <response code="200">
    /// Successful request with data as follows:
    /// <code>
    /// {
    ///     accountId: string,
    ///     roles: Array:number, // expects an array of enums
    ///     bearerToken: string,
    ///     authorizationToken: string,
    ///     refreshToken: string,
    ///     authorizedTimestamp: number,
    ///     validityDuration: number,
    ///     twoFactorConfirmed?: boolean,
    ///     isPreAuthorization: false,
    /// }
    /// </code>
    /// </response>
    /// <response code="400">Bad Request - The validation for login data was failed.</response>
    /// <response code="422">Unprocessable Content - The Account has not been activated.</response>
    /// <response code="409">
    /// Conflict - The Password is incorrect.
    /// This Status Code comes with the following Response Body:
    /// <code>
    /// {
    ///     loginFailedCount: number,
    ///     lockOutCount: number,
    /// }
    /// </code>
    /// </response>
    /// <response code="423">
    /// Locked - The Account has been locked out or suspended due to many attempts of failed logins.
    /// This Status Code comes with the following Response Body:
    /// <code>
    /// {
    ///     isSuspended: boolean,
    ///     timestamp: number,
    ///     loginFailedCount: number,
    ///     lockOutCount: number,
    /// }
    /// </code>
    /// </response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPost("authenticate-by-credentials")]
    public async Task<IActionResult> AuthenticateByCredentials([FromBody] AuthenticationData authenticationData) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(AuthenticateByCredentials) });

        var errors = await authenticationData.VerifyAuthenticationData(_phoneNumberHandler);
        if (errors.Count != 0) return new ErrorResponse(HttpStatusCode.BadRequest, errors);
        
        var (account, profile) = await GetAccountAndProfileByLoginInformation(new LoginInformation { EmailAddress = authenticationData.EmailAddress, PhoneNumber = authenticationData.PhoneNumber });
        
        var authenticateByEmailAddress = authenticationData.EmailAddress.IsString();
        var dataError = VerifyAccountAndProfileData(account, profile, authenticateByEmailAddress);
        if (dataError is not null) return dataError;

        // Todo: implement trusted device authorization

        if (
            (
                account!.LockOutEnabled &&
                account.LockOutOn!.Value.Compute(_haloConfigs.LockOutDuration, _haloConfigs.LockOutDurationUnit) <= DateTime.UtcNow
            ) || account.IsSuspended
        ) return new ErrorResponse(HttpStatusCode.Locked, new {
            isSuspended = account.IsSuspended,
            timestamp = account.LockOutOn!.Value.ToTimestamp(),
            loginFailedCount = account.LoginFailedCount,
            lockOutCount = account.LockOutCount,
        });
        
        var isPasswordMatched = _cryptoService.IsHashMatchesPlainText(account.HashPassword, authenticationData.Password);

        if (!isPasswordMatched) {
            var result = await UpdateLockoutAndSuspendOnFailedLogin(account);
            if (!result.HasValue || !result.Value) return new ErrorResponse();
            return account.LockOutEnabled || account.IsSuspended
                ? new ErrorResponse(HttpStatusCode.Locked, new {
                    isSuspended = account.IsSuspended,
                    timestamp = account.LockOutOn!.Value.ToTimestamp(),
                    loginFailedCount = account.LoginFailedCount,
                    lockOutCount = account.LockOutCount,
                })
                : new ErrorResponse(HttpStatusCode.Conflict, new {
                    loginFailedCount = account.LoginFailedCount,
                    lockOutCount = account.LockOutCount,
                });
        }

        await _contextService.StartTransaction();

        var resetLockoutResult = await ResetLockoutAndSuspendOnSuccessfulLogin(account);
        if (!resetLockoutResult.HasValue || !resetLockoutResult.Value) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        var authorization = await CreateAuthorization(account, profile!);
        if (authorization is null) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        await _contextService.ConfirmTransaction();
        return new SuccessResponse(authorization);
    }

    /// <summary>
    /// For Customer. To perform login using a One-Time Password.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     POST /authenticate-by-credentials
    ///     Body
    ///         {
    ///             emailAddress?: string,
    ///             phoneNumber?: {
    ///                 regionCode: string,
    ///                 phoneNumber: string,
    ///             },
    ///         }
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="loginInformation">The data required for login.</param>
    /// <response code="200">
    /// Successful request with data as follows:
    /// <code>
    /// {
    ///     accountId: string,
    ///     authorizationToken: string,
    ///     authorizedTimestamp: number,
    ///     isPreAuthorization: true,
    /// }
    /// </code>
    /// </response>
    /// <response code="400">Bad Request - The validation for login data was failed.</response>
    /// <response code="422">Unprocessable Content - The Account has not been activated.</response>
    /// <response code="423">
    /// Locked - The Account has been locked out or suspended due to many attempts of failed logins.
    /// This Status Code comes with the following Response Body:
    /// <code>
    /// {
    ///     isSuspended: boolean,
    ///     timestamp: number,
    ///     loginFailedCount: number,
    ///     lockOutCount: number,
    /// }
    /// </code>
    /// </response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPost("authenticate-by-otp")]
    public async Task<IActionResult> AuthenticateByOneTimePassword([FromBody] LoginInformation loginInformation) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(AuthenticateByOneTimePassword) });

        var errors = await loginInformation.VerifyLoginInformation(_phoneNumberHandler);
        if (errors.Count != 0) return new ErrorResponse(HttpStatusCode.BadRequest, errors);

        var (account, profile) = await GetAccountAndProfileByLoginInformation(new LoginInformation { EmailAddress = loginInformation.EmailAddress, PhoneNumber = loginInformation.PhoneNumber });
        
        var authenticateByEmailAddress = loginInformation.EmailAddress.IsString();
        var dataError = VerifyAccountAndProfileData(account, profile, authenticateByEmailAddress);
        if (dataError is not null) return dataError;

        // Todo: implement trusted device authorization, check if it should be done here or the method below
        
        if (
            (
                account!.LockOutEnabled &&
                account.LockOutOn!.Value.Compute(_haloConfigs.LockOutDuration, _haloConfigs.LockOutDurationUnit) <= DateTime.UtcNow
            ) || account.IsSuspended
        ) return new ErrorResponse(HttpStatusCode.Locked, new {
            isSuspended = account.IsSuspended,
            timestamp = account.LockOutOn!.Value.ToTimestamp(),
            loginFailedCount = account.LoginFailedCount,
            lockOutCount = account.LockOutCount,
        });

        account.OneTimePassword = StringHelpers.GenerateRandomString(NumberHelpers.GetRandomNumberInRangeInclusive(_haloConfigs.OtpMinLength, _haloConfigs.OtpMaxLength));
        account.OneTimePasswordTimestamp = DateTime.UtcNow;

        await _contextService.StartTransaction();
        var updateAccountResult = await _accountService.UpdateAccount(account);
        if (!updateAccountResult.HasValue || !updateAccountResult.Value) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        var shouldSendOneTimePasswordToEmail = (!authenticateByEmailAddress && account.EmailAddress.IsString() && account.EmailConfirmed) || (
            authenticateByEmailAddress && (!profile!.PhoneNumber.IsString() || !profile.PhoneNumberConfirmed)
        );

        if (shouldSendOneTimePasswordToEmail) {
            var mailBinding = new MailBinding {
                ToReceivers = [new Recipient { EmailAddress = account.EmailAddress! }],
                Title = $"{Constants.ProjectName}: Your OTP",
                Priority = MailPriority.High,
                TemplateName = Enums.EmailTemplate.OneTimePasswordEmail,
                Placeholders = new Dictionary<string, string> {
                    { "EMAIL_ADDRESS", account.EmailAddress! },
                    { "ONE_TIME_PASSWORD", Uri.EscapeDataString(account.OneTimePassword) },
                    { "VALIDITY_DURATION", _haloConfigs.OtpValidityDuration.ToString() },
                    { "VALIDITY_UNIT", _haloConfigs.OtpValidityDurationUnit.GetValue()! }
                }
            };

            var emailSendingResult = await _mailService.SendSingleEmail(mailBinding);
            if (!emailSendingResult) {
                await _contextService.RevertTransaction();
                return new ErrorResponse();
            }
        }
        else {
            var smsContent = _haloConfigs.OneTimePasswordSmsContent
                             .Replace("ONE_TIME_PASSWORD", account.OneTimePassword)
                             .Replace("VALIDITY_DURATION", $"{_haloConfigs.OtpValidityDuration} {_haloConfigs.OtpValidityDurationUnit.GetValue()}");

            var phoneNumber = JsonConvert.DeserializeObject<RegionalizedPhoneNumber>(profile!.PhoneNumber!);
            var smsBinding = new SingleSmsBinding {
                SmsContent = smsContent,
                Receivers = [phoneNumber!.ToString()]
            };

            var smsResult = await _clickatellSmsHttpService.SendSingleSms(smsBinding);
            if (smsResult is null || smsResult.Length != 0) {
                await _contextService.RevertTransaction();
                return new ErrorResponse();
            }
        }

        var preAuthorization = new Authorization {
            AccountId = account.Id,
            AuthorizationToken = await _cryptoService.CreateSha512Hash(StringHelpers.GenerateRandomString(Constants.RandomStringDefaultLength, true)),
            AuthorizedTimestamp = account.OneTimePasswordTimestamp.Value.ToTimestamp(),
            IsPreAuthorization = true,
        };
        HttpContext.Session.SetString(Enums.SessionKey.PreAuthorization.GetValue()!, JsonConvert.SerializeObject(preAuthorization));

        await _contextService.ConfirmTransaction();
        return new SuccessResponse(preAuthorization);
    }

    /// <summary>
    /// For Customer. To perform login using a One-Time Password.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     POST /authenticate-by-credentials
    ///     Body
    ///         {
    ///             emailAddress?: string,
    ///             phoneNumber?: {
    ///                 regionCode: string,
    ///                 phoneNumber: string,
    ///             },
    ///             password: string,
    ///             isTrusted: boolean,
    ///             deviceInformation?: {
    ///                 name?: string,
    ///                 deviceType?: string,
    ///                 uniqueIdentifier?: string,
    ///                 uniqueIdentifierType?: string,
    ///                 location?: string,
    ///                 ipAddress?: string,
    ///                 operatingSystem?: string,
    ///                 browserType?: string,
    ///             },
    ///         }
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="accountId">The Account ID of the Pre-Authenticated User.</param>
    /// <param name="oneTimePassword">The One-Time Password.</param>
    /// <response code="200">
    /// Successful request with data as follows:
    /// <code>
    /// {
    ///     accountId: string,
    ///     roles: Array:number, // expects an array of enums
    ///     bearerToken: string,
    ///     authorizationToken: string,
    ///     refreshToken: string,
    ///     authorizedTimestamp: number,
    ///     validityDuration: number,
    ///     twoFactorConfirmed?: boolean,
    ///     isPreAuthorization: false,
    /// }
    /// </code>
    /// </response>
    /// <response code="410">Gone - The Pre-Authenticated data has expired.</response>
    /// <response code="409">
    /// Conflict - The Password is incorrect.
    /// This Status Code comes with the following Response Body:
    /// <code>
    /// {
    ///     loginFailedCount: number,
    ///     lockOutCount: number,
    /// }
    /// </code>
    /// </response>
    /// <response code="423">
    /// Locked - The Account has been locked out or suspended due to many attempts of failed logins.
    /// This Status Code comes with the following Response Body:
    /// <code>
    /// {
    ///     isSuspended: boolean,
    ///     timestamp: number,
    ///     loginFailedCount: number,
    ///     lockOutCount: number,
    /// }
    /// </code>
    /// </response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [ServiceFilter(typeof(AuthenticatedAuthorize))]
    //[ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOneTimePassword([FromHeader] string accountId, [FromHeader] string oneTimePassword) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(VerifyOneTimePassword) });

        var sessionPreAuthorization = HttpContext.Session.GetString(Enums.SessionKey.PreAuthorization.GetValue()!);
        if (!sessionPreAuthorization.IsString()) return new ErrorResponse(HttpStatusCode.Gone);
        
        var preAuthorization = JsonConvert.DeserializeObject<Authorization>(sessionPreAuthorization!);
        if (preAuthorization is null) return new ErrorResponse();
        
        var account = await _accountService.GetAccountById(accountId);
        if (account is null) return new ErrorResponse();

        HttpContext.Session.Remove(Enums.SessionKey.PreAuthorization.GetValue()!);
        
        if (
            (
                account.LockOutEnabled &&
                account.LockOutOn!.Value.Compute(_haloConfigs.LockOutDuration, _haloConfigs.LockOutDurationUnit) <= DateTime.UtcNow
            ) || account.IsSuspended
        ) return new ErrorResponse(HttpStatusCode.Locked, new {
            isSuspended = account.IsSuspended,
            timestamp = account.LockOutOn!.Value.ToTimestamp(),
            loginFailedCount = account.LoginFailedCount,
            lockOutCount = account.LockOutCount,
        });

        var isOneTimePasswordMatchedAndValid =
            Equals(account.OneTimePassword, oneTimePassword) &&
            preAuthorization.AuthorizedTimestamp + _haloConfigs.OtpValidityDuration.ToMilliseconds(_haloConfigs.OtpValidityDurationUnit) < DateTime.UtcNow.ToTimestamp();

        if (!isOneTimePasswordMatchedAndValid) {
            var result = await UpdateLockoutAndSuspendOnFailedLogin(account);
            if (!result.HasValue || !result.Value) return new ErrorResponse();
            return account.LockOutEnabled || account.IsSuspended
                ? new ErrorResponse(HttpStatusCode.Locked, new {
                    isSuspended = account.IsSuspended,
                    timestamp = account.LockOutOn!.Value.ToTimestamp(),
                    loginFailedCount = account.LoginFailedCount,
                    lockOutCount = account.LockOutCount,
                })
                : new ErrorResponse(HttpStatusCode.Conflict, new {
                    loginFailedCount = account.LoginFailedCount,
                    lockOutCount = account.LockOutCount,
                });
        }

        var profile = await _profileService.GetProfileByAccountId(accountId);
        if (profile is null) return new ErrorResponse();

        await _contextService.StartTransaction();
        
        var resetLockoutResult = await ResetLockoutAndSuspendOnSuccessfulLogin(account);
        if (!resetLockoutResult.HasValue || !resetLockoutResult.Value) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }
        
        var authorization = await CreateAuthorization(account, profile);
        if (authorization is null) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        await _contextService.ConfirmTransaction();
        return new SuccessResponse(authorization);
    }

    /// <summary>
    /// Endpoint not implemented yet. Intended to automatically create user authentication data and session using cookies.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPost("authenticate-by-cookies")]
    public async Task<IActionResult> AuthenticateByCookies() {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(AuthenticateByCookies) });
        throw new NotImplementedException();
    }

    /// <summary>
    /// For guest. To request a Token to recover their Account due to forgotten password.
    /// The Account must have been activated, and is not locked out or suspended from using login feature.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     POST /forgot-password
    ///     Headers
    ///         RecaptchaToken: string
    ///     Body
    ///         {
    ///             emailAddress?: string,
    ///             phoneNumber?: {
    ///                 regionCode: string,
    ///                 phoneNumber: string,
    ///             },
    ///         }
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="loginInformation">The data required to recover password.</param>
    /// <response code="200">Successful request.</response>
    /// <response code="400">Bad Request - The validation for credential data was failed.</response>
    /// <response code="423">Locked - The Account is not yet activated, or is locked out or suspended.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] LoginInformation loginInformation) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(ForgotPassword) });
        
        var errors = await loginInformation.VerifyLoginInformation(_phoneNumberHandler);
        if (errors.Count != 0) return new ErrorResponse(HttpStatusCode.BadRequest, errors);

        var relyOnAccount = loginInformation.EmailAddress.IsString();
        var (account, profile) = await GetAccountAndProfileByLoginInformation(loginInformation, !relyOnAccount);
        
        if (account is null || (!relyOnAccount && profile is null)) return new ErrorResponse();
        
        var shouldProcessForgotPassword = ((relyOnAccount && account.EmailConfirmed) || (!relyOnAccount && profile!.PhoneNumberConfirmed)) && !account.IsSuspended;
        if (!shouldProcessForgotPassword) return new ErrorResponse(HttpStatusCode.Locked);

        account.RecoveryToken = StringHelpers.GenerateRandomString(NumberHelpers.GetRandomNumberInRangeInclusive(_haloConfigs.RecoveryTokenMinLength, _haloConfigs.RecoveryTokenMaxLength), true);
        account.RecoveryTokenTimestamp = DateTime.UtcNow;

        await _contextService.StartTransaction();
        var updateAccountResult = await _accountService.UpdateAccount(account);
        if (!updateAccountResult.HasValue || !updateAccountResult.Value) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        bool tokenSendingResult;
        if (relyOnAccount) {
            var validityUntil = account.RecoveryTokenTimestamp.Value
                                       .Compute(_haloConfigs.RecoveryTokenValidityDuration, _haloConfigs.RecoveryTokenValidityDurationUnit)
                                       .Format(Enums.DateFormat.DDMMYYYYS, Enums.TimeFormat.HHMMTTC);
            
            var emailBinding = new MailBinding {
                ToReceivers = [new Recipient { EmailAddress = account.EmailAddress! }],
                Title = $"{Constants.ProjectName}: Recover your account",
                Priority = MailPriority.High,
                TemplateName = Enums.EmailTemplate.AccountRecoveryEmail,
                Placeholders = new Dictionary<string, string> {
                    { "EMAIL_ADDRESS", account.EmailAddress! },
                    { "RECOVERY_TOKEN", Uri.EscapeDataString(account.RecoveryToken) },
                    { "VALID_UNTIL_DATETIME", validityUntil }
                }
            };

            tokenSendingResult = await _mailService.SendSingleEmail(emailBinding);
        }
        else {
            var smsContent = _haloConfigs.AccountRecoverySmsContent
                             .Replace("ACCOUNT_RECOVERY_TOKEN", account.RecoveryToken)
                             .Replace("VALIDITY_DURATION", $"{_haloConfigs.RecoveryTokenValidityDuration} {_haloConfigs.RecoveryTokenValidityDurationUnit}");
            
            var smsBinding = new SingleSmsBinding {
                SmsContent = smsContent,
                Receivers = [profile!.PhoneNumber!]
            };

            var smsSendingResult = await _clickatellSmsHttpService.SendSingleSms(smsBinding);
            tokenSendingResult = smsSendingResult is not null && smsSendingResult.Length == 0;
        }

        if (!tokenSendingResult) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        await _contextService.ConfirmTransaction();
        return new SuccessResponse();
    }

    /// <summary>
    /// For guest. To set new password after requesting to recover their account due to forgotten password.
    /// The Account must have been activated, and is not locked out or suspended from using login feature.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     PATCH /recover-account/recoveryToken={string}
    ///     Headers
    ///         RecaptchaToken: string
    ///     Body
    ///         {
    ///             emailAddress?: string,
    ///             phoneNumber?: {
    ///                 regionCode: string,
    ///                 phoneNumber: string,
    ///             },
    ///             password: string,
    ///         }
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="registrationData">The data required to reset password.</param>
    /// <param name="recoveryToken">The Token required to reactivate account.</param>
    /// <response code="200">Successful request.</response>
    /// <response code="400">Bad Request - The validation for registration data was failed.</response>
    /// <response code="409">Conflict - The recovery Token is mismatched or expired.</response>
    /// <response code="423">Locked - The Account is not yet activated, or is locked out or suspended.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPatch("recover-account/{recoveryToken}")]
    public async Task<IActionResult> RecoverAccount([FromBody] RegistrationData registrationData, [FromRoute] string recoveryToken) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(RecoverAccount) });
        
        var errors = await registrationData.VerifyRegistrationData(_phoneNumberHandler);
        if (errors.Count != 0) return new ErrorResponse(HttpStatusCode.BadRequest, errors);
        
        var relyOnAccount = registrationData.EmailAddress.IsString();
        var (account, profile) = await GetAccountAndProfileByLoginInformation(registrationData, !relyOnAccount);
        
        if (account is null || (!relyOnAccount && profile is null)) return new ErrorResponse();
        
        var shouldRecoverAccount = ((relyOnAccount && account.EmailConfirmed) || (!relyOnAccount && profile!.PhoneNumberConfirmed)) && !account.IsSuspended;
        if (!shouldRecoverAccount) return new ErrorResponse(HttpStatusCode.Locked);

        if (
            !Equals(account.RecoveryToken, Uri.UnescapeDataString(recoveryToken)) ||
            account.RecoveryTokenTimestamp!.Value.Compute(_haloConfigs.RecoveryTokenValidityDuration, _haloConfigs.RecoveryTokenValidityDurationUnit) > DateTime.UtcNow
        ) return new ErrorResponse(HttpStatusCode.Conflict);

        account.RecoveryToken = null;
        account.RecoveryTokenTimestamp = null;
        
        var (_, saltLength) = GetTokenLengthsForNewAccount();
        var (hashedPassword, salt) = _cryptoService.GenerateHashAndSalt(registrationData.Password, saltLength);
        
        account.PasswordSalt = salt;
        account.HashPassword = hashedPassword;
        
        var result = await _accountService.UpdateAccount(account);
        return !result.HasValue || !result.Value ? new ErrorResponse() : new SuccessResponse();
    }

    /// <summary>
    /// For all roles. To enable or renew the Two-Factor Authentication on their Account. The user must have been logged in.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     PATCH /enable-or-renew-two-factor-authentication
    ///     Headers
    ///         AccountId: string
    ///         Authorization: "Bearer {string}"
    ///         AccessToken: string
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="accountId">The ID of Account to have Two-Factor Authentication feature enabled or renewed.</param>
    /// <response code="200">
    /// Successful request with data as follows:
    /// <code>
    /// {
    ///     qrCodeImageUrl: string,
    ///     manualEntryKey: string,
    ///     verifyingTokens: Array:string,
    /// }
    /// </code>
    /// </response>
    /// <response code="422">Unprocessable Entity - The AccountId not referenced any records in database.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [ServiceFilter(typeof(AuthenticatedAuthorize))]
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    // [ServiceFilter(typeof(TwoFactorAuthorize))]
    [HttpPatch("enable-or-renew-two-factor-authentication")]
    public async Task<IActionResult> EnableOrRenewTwoFactorAuthentication([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(EnableOrRenewTwoFactorAuthentication) });

        var account = await _accountService.GetAccountById(accountId);
        if (account is null) return new ErrorResponse();

        Profile? profile = null;
        if (!account.EmailAddress.IsString()) {
            profile = await _profileService.GetProfileByAccountId(accountId);
            if (profile is null) return new ErrorResponse();
            if (!profile.PhoneNumber.IsString()) return new ErrorResponse(HttpStatusCode.UnprocessableEntity);
        }

        var twoFactorSecretKey = StringHelpers.GenerateRandomString(NumberHelpers.GetRandomNumberInRangeInclusive(_haloConfigs.TfaKeyMinLength, _haloConfigs.TfaKeyMaxLength));
        var twoFactorData = _twoFactorService.GetTwoFactorAuthenticationData(new GetTwoFactorBinding {
            EmailAddress = account.EmailAddress ??
                           JsonConvert.DeserializeObject<RegionalizedPhoneNumber>(profile!.PhoneNumber!)!.ToString().Replace(Constants.MultiSpace, string.Empty),
            SecretKey = twoFactorSecretKey
        });

        account.TwoFactorEnabled = true;
        account.TwoFactorKeys = JsonConvert.SerializeObject(new TwoFactorKeys { SecretKey = twoFactorSecretKey, ManualEntryKey = twoFactorData.ManualEntryKey });

        var twoFactorVerifyingTokens = new int[5]
                                     .Select(_ => StringHelpers.GenerateRandomString(20, false, false))
                                     .Select(x => x.SplitToGroups(5))
                                     .ToArray();
        account.TwoFactorVerifyingTokens = JsonConvert.SerializeObject(twoFactorVerifyingTokens);

        var result = await _accountService.UpdateAccount(account);
        return !result.HasValue || !result.Value
            ? new ErrorResponse()
            : new SuccessResponse(new {
                qrCodeImageUrl = twoFactorData.QrCodeImageUrl,
                manualEntryKey = twoFactorData.ManualEntryKey,
                verifyingTokens = twoFactorVerifyingTokens,
            });
    }
    
    /// <summary>
    /// For all roles. To disable the Two-Factor Authentication on their Account. The user must have been logged in.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     PATCH /disable-two-factor-authentication/twoFactorVerifyingToken={string}
    ///     Headers
    ///         AccountId: string
    ///         Authorization: "Bearer {string}"
    ///         AccessToken: string
    ///         RecaptchaToken: string
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="twoFactorVerifyingToken">The Token that was issued when enabling the feature.</param>
    /// <param name="accountId">The ID of Account to have Two-Factor Authentication disabled.</param>
    /// <response code="200">Successful request.</response>
    /// <response code="400">Bad Request - The feature is not yet enabled.</response>
    /// <response code="403">Forbidden - The verifying Token is mismatched.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [ServiceFilter(typeof(AuthenticatedAuthorize))]
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [ServiceFilter(typeof(TwoFactorAuthorize))]
    [HttpPatch("disable-two-factor-authentication/{twoFactorVerifyingToken}")]
    public async Task<IActionResult> DisableTwoFactorAuthentication([FromHeader] string accountId, [FromRoute] string twoFactorVerifyingToken) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(DisableTwoFactorAuthentication) });
        
        var account = await _accountService.GetAccountById(accountId);
        if (account is null) return new ErrorResponse();
        if (!account.TwoFactorEnabled) return new ErrorResponse(HttpStatusCode.BadRequest);

        var twoFactorVerifyingTokens = JsonConvert.DeserializeObject<string[]>(account.TwoFactorVerifyingTokens!);
        if (twoFactorVerifyingTokens is null) return new ErrorResponse();
        if (!twoFactorVerifyingTokens.Any(x => Equals(x, twoFactorVerifyingToken))) return new ErrorResponse(HttpStatusCode.Forbidden);

        account.TwoFactorEnabled = false;
        account.TwoFactorKeys = null;
        account.TwoFactorVerifyingTokens = null;

        var result = await _accountService.UpdateAccount(account);
        return !result.HasValue || !result.Value ? new ErrorResponse() : new SuccessResponse();
    }

    /// <summary>
    /// For all roles. To verify the Two-Factor Token. The user must have been logged in.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     POST /verify-tfa
    ///     Headers
    ///         AccountId: string
    ///         Authorization: "Bearer {string}"
    ///         AccessToken: string
    ///         RecaptchaToken: string
    ///         TwoFactorToken: string
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="twoFactorToken">The Two-Factor Token sent from client to be verified.</param>
    /// <param name="accountId">The ID of Account to have Two-Factor Token verified.</param>
    /// <response code="200">Successful request.</response>
    /// <response code="100">Continue - The Two-Factor Authentication service or the Account's Two-Factor setting is not enabled.</response>
    /// <response code="400">Bad Request - The Two-Factor Token is missing in the request.</response>
    /// <response code="401">Unauthorized - The Two-Factor Token validation is failed.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    /// <response code="501">Not Implemented - The Two-Factor secret key can't be found from database.</response>
    [ServiceFilter(typeof(AuthenticatedAuthorize))]
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPost("verify-tfa")]
    public async Task<IActionResult> VerifyTwoFactorToken([FromHeader] string accountId, [FromHeader] string twoFactorToken) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(VerifyTwoFactorToken) });
        
        if (!twoFactorToken.IsString()) return new ErrorResponse(HttpStatusCode.BadRequest);
        if (!_haloConfigs.TfaEnabled) return new ErrorResponse(HttpStatusCode.Continue);
        
        var account = await _accountService.GetAccountById(accountId);
        if (account is null) return new ErrorResponse();
        if (!account.TwoFactorEnabled) return new ErrorResponse(HttpStatusCode.Continue);
        if (!account.TwoFactorKeys.IsString()) return new ErrorResponse(HttpStatusCode.NotImplemented);
        
        var twoFactorKeys = JsonConvert.DeserializeObject<TwoFactorKeys>(account.TwoFactorKeys!);
        if (twoFactorKeys is null) return new ErrorResponse();

        var isTwoFactorTokenValid = _twoFactorService.VerifyTwoFactorAuthenticationPin(new VerifyTwoFactorBinding {
            PinCode = twoFactorToken,
            SecretKey = twoFactorKeys.SecretKey,
        });
        
        return !isTwoFactorTokenValid ? new ErrorResponse(HttpStatusCode.Unauthorized) : new SuccessResponse();
    }

    private async Task<Tuple<Account?, Profile?>> GetAccountAndProfileByLoginInformation(LoginInformation loginInformation, bool shouldGetAdditionalData = true) {
        var authenticateByEmailAddress = loginInformation.EmailAddress.IsString();

        Account? account = null;
        Profile? profile = null;
        if (authenticateByEmailAddress) {
            account = await _accountService.GetAccountByEmailAddress(loginInformation.EmailAddress!);
            if (account is not null && shouldGetAdditionalData) profile = await _profileService.GetProfileByAccountId(account.Id);
        }
        else {
            profile = await _profileService.GetProfileByPhoneNumber(loginInformation.PhoneNumber!);
            if (profile is not null && shouldGetAdditionalData) account = await _accountService.GetAccountById(profile.AccountId);
        }

        return new Tuple<Account?, Profile?>(account, profile);
    }

    private static IActionResult? VerifyAccountAndProfileData(Account? account, Profile? profile, bool authenticatedByEmail = true) {
        if (account is null || profile is null) return new ErrorResponse();

        if (authenticatedByEmail)
            return !account.EmailConfirmed
                ? new ErrorResponse(HttpStatusCode.UnprocessableContent)
                : default;
        
        return !profile.PhoneNumberConfirmed
            ? new ErrorResponse(HttpStatusCode.UnprocessableContent)
            : default;
    }

    private async Task<bool?> UpdateLockoutAndSuspendOnFailedLogin(Account account) {
        account.LoginFailedCount += 1;
        if (account.LoginFailedCount < _haloConfigs.LoginFailedThreshold) {
            account.LockOutEnabled = false;
            account.LockOutOn = null;
        }
        else {
            account.LockOutEnabled = true;
            account.LockOutOn = DateTime.UtcNow;
            account.LockOutCount += 1;
            
            if (account.LockOutCount < _haloConfigs.LockOutThreshold)
                account.LoginFailedCount = 0;
            else account.IsSuspended = true;
        }
        if (account.LockOutEnabled || account.IsSuspended) HttpContext.Session.Clear();
        // Todo: Send email to notify Suspend status

        return await _accountService.UpdateAccount(account);
    }

    private async Task<bool?> ResetLockoutAndSuspendOnSuccessfulLogin(Account account) {
        account.LockOutEnabled = false;
        account.LockOutOn = null;
        account.LockOutCount = 0;
        account.LoginFailedCount = 0;
        account.IsSuspended = false;
        
        return await _accountService.UpdateAccount(account);
    }

    private async Task<Authorization?> CreateAuthorization(Account account, Profile profile) {
        var roles = await _roleService.GetAllAccountRoles(account.Id);
        if (roles is null) return null;
        
        var authenticatedUser = await _accountService.GetInformationForAuthenticatedUser(account.Id);
        if (authenticatedUser is null) return null;
        
        var preference = await _preferenceService.GetPreference(account.Id);
        if (preference is null) return null;
        
        var jwtToken = _jwtService.GenerateRequestAuthenticationToken(new Dictionary<string, string> {
            { ClaimTypes.Actor, account.Id },
            { ClaimTypes.Authentication, true.ToString() },
            { ClaimTypes.Role, string.Join(Constants.Comma, roles) },
            { ClaimTypes.Email, account.EmailAddress ?? string.Empty },
            { ClaimTypes.Gender, profile.Gender.ToString() },
            { ClaimTypes.Name, $"{profile.GivenName}{profile.MiddleName}{profile.LastName}" },
            { ClaimTypes.MobilePhone, profile.PhoneNumber ?? string.Empty },
            { ClaimTypes.SerialNumber, account.UniqueCode },
            { ClaimTypes.DateOfBirth, profile.DateOfBirth.HasValue ? profile.DateOfBirth.Value.ToString(CultureInfo.InvariantCulture) : string.Empty },
        });

        var authenticatedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var authorization = new Authorization {
            AccountId = account.Id,
            Roles = roles,
            AuthorizationToken = await _cryptoService.CreateSha512Hash(StringHelpers.GenerateRandomString(Constants.RandomStringDefaultLength, true)),
            AuthorizedTimestamp = authenticatedTimestamp,
            BearerToken = jwtToken,
            RefreshToken = await _cryptoService.CreateSha512Hash(StringHelpers.GenerateRandomString(Constants.RandomStringDefaultLength, true)),
            ValidityDuration = _haloConfigs.AuthenticationValidityDuration.ToMilliseconds(_haloConfigs.AuthenticationValidityDurationUnit),
            TwoFactorConfirmed = !_haloConfigs.TfaEnabled
                ? null
                : account.TwoFactorEnabled ? false : null,
            IsPreAuthorization = false,
        };
        
        Response.Cookies.Append(nameof(Authorization.AuthorizedTimestamp), authenticatedTimestamp.ToString(), _cookieOptions);
        Response.Cookies.Append(nameof(Authorization.RefreshToken), authorization.RefreshToken, _cookieOptions);
        HttpContext.Session.SetString(Enums.SessionKey.Authorization.GetValue()!, JsonConvert.SerializeObject(authorization));
        
        HttpContext.Session.SetString(Enums.SessionKey.AuthenticatedUser.GetValue()!, JsonConvert.SerializeObject(authenticatedUser));
        
        var preferenceSettings = (Bindings.ServiceBindings.Preference)preference;
        HttpContext.Session.SetString(Enums.SessionKey.Preference.GetValue()!, JsonConvert.SerializeObject(preferenceSettings));

        return authorization;
    }

    private Tuple<int, int> GetTokenLengthsForNewAccount(bool forSaltOnly = false) {
        var saltLength = NumberHelpers.GetRandomNumberInRangeInclusive(_haloConfigs.SaltMinLength, _haloConfigs.SaltMaxLength);
        if (forSaltOnly) return new Tuple<int, int>(0, saltLength);
        
        var verificationTokenLength = NumberHelpers.GetRandomNumberInRangeInclusive(_haloConfigs.EmailTokenMinLength, _haloConfigs.EmailTokenMaxLength);
        return new Tuple<int, int>(verificationTokenLength, saltLength);
    }
}
