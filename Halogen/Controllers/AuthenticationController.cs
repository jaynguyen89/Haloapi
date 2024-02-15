using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;
using AssistantLibrary;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Interfaces.IServiceFactory;
using AssistantLibrary.Services;
using Halogen.Bindings.ApiBindings;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;
using Halogen.Attributes;
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
    private readonly IAssistantService _assistantService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly HttpContext? _httpContext;

    private readonly HalogenConfigs _configs;
    private readonly RegionalizedPhoneNumberHandler _phoneNumberHandler;

    public AuthenticationController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IHaloServiceFactory haloServiceFactory,
        IAssistantServiceFactory assistantServiceFactory,
        IHaloConfigProvider haloConfigProvider,
        RegionalizedPhoneNumberHandler phoneNumberHandler
    ) : base(ecosystem, logger, configuration) {
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
        
        _assistantService = assistantServiceFactory.GetService<AssistantService>() ?? throw new HaloArgumentNullException<AuthenticationController>(nameof(AssistantService));
        _clickatellSmsHttpService = assistantServiceFactory.GetService<SmsServiceFactory>()?.GetActiveSmsService() ?? throw new HaloArgumentNullException<AuthenticationController>(nameof(SmsServiceFactory));
        _twoFactorService = assistantServiceFactory.GetService<TwoFactorService>() ?? throw new HaloArgumentNullException<AuthenticationController>(nameof(TwoFactorService));
        _httpContext = httpContextAccessor.HttpContext;

        _configs = haloConfigProvider.GetHalogenConfigs();
        _phoneNumberHandler = phoneNumberHandler;
    }

    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPost("register-account")]
    public async Task<IActionResult> RegisterAccount([FromBody] RegistrationData registrationData) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(RegisterAccount) });

        var errors = await registrationData.VerifyRegistrationData(_phoneNumberHandler);
        if (errors.Any()) return new ErrorResponse(HttpStatusCode.BadRequest, errors);

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
        
        var newProfile = Profile.CreateNewProfile(_useLongerId, accountId, registerByEmailAddress, _configs.PhoneTokenMinLength, _configs.PhoneTokenMaxLength, registrationData.PhoneNumber, registrationData.ProfileData);
        
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

        var defaultPreference = Preference.CreatePreferenceForNewAccount(_useLongerId, accountId);
        var preferenceId = await _preferenceService.InsertNewPreference(defaultPreference);
        if (preferenceId is null) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        if (registerByEmailAddress) {
            var accountActivationEmailExpiry = newAccount.EmailAddressTokenTimestamp!.Value
                                                         .Compute(_configs.EmailTokenValidityDuration, _configs.EmailTokenValidityDurationUnit)
                                                         .Format(Enums.DateFormat.DDMMYYYYS, Enums.TimeFormat.HHMMTTC);
            
            var mailBinding = new MailBinding {
                ToReceivers = new List<Recipient> { new() { EmailAddress = newAccount.EmailAddress! } },
                Title = $"{Constants.ProjectName}: Activate your account",
                Priority = MailPriority.High,
                TemplateName = Enums.EmailTemplate.AccountActivationEmail,
                Placeholders = new Dictionary<string, string> {
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
            var smsContent = _configs.AccountActivationSmsContent
                             .Replace("CLIENT_BASE_URI", _configs.ClientBaseUri)
                             .Replace("PHONE_NUMBER", registrationData.PhoneNumber!.Simplify())
                             .Replace("REGISTRATION_TOKEN", Uri.EscapeDataString(newProfile.PhoneNumberToken!))
                             .Replace("USER_NAME", newAccount.Username is null ? string.Empty : Uri.EscapeDataString(newAccount.Username))
                             .Replace("VALIDITY_DURATION", $"{_configs.PhoneTokenValidityDuration} {_configs.PhoneTokenValidityDurationUnit}s");
            var smsBinding = new SingleSmsBinding {
                SmsContent = smsContent,
                Receivers = new List<string> { registrationData.PhoneNumber!.ToString() },
            };

            var smsResult = await _clickatellSmsHttpService.SendSingleSms(smsBinding);
            if (smsResult is null || smsResult.Any()) {
                await _contextService.RevertTransaction();
                return new ErrorResponse();
            }
        }

        await _contextService.ConfirmTransaction();
        return new SuccessResponse(HttpStatusCode.Created);
    }

    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpGet("send-secret-code/{destination}")]
    public async Task<IActionResult> SendSecretCode([FromRoute] string destination) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(SendSecretCode) });
        
        var isPhoneNumber = new Regex(@"^[\d]{2,2},[\d]{9,9}$").IsMatch(destination);
        var account = await (isPhoneNumber ? _profileService.GetAccountByPhoneNumber(RegionalizedPhoneNumber.Convert(destination)) : _accountService.GetAccountByEmailAddress(destination));
        if (account is null) return new ErrorResponse(HttpStatusCode.UnprocessableEntity);

        if (isPhoneNumber) {
            var profile = await _profileService.GetProfileByPhoneNumber(RegionalizedPhoneNumber.Convert(destination));
            if (profile is null) return new ErrorResponse(HttpStatusCode.UnprocessableEntity);
            
            if (!profile.PhoneNumberToken.IsString() || !profile.PhoneNumberTokenTimestamp.HasValue) return new ErrorResponse(HttpStatusCode.NotFound);
            if (profile.PhoneNumberTokenTimestamp.Value > DateTime.UtcNow) return new ErrorResponse(HttpStatusCode.Gone);
        }
        else {
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

        if (isPhoneNumber) {
            var smsContent = _configs.SecretCodeSmsContent
                .Replace("SECRET_CODE", account.SecretCode)
                .Replace("VALIDITY_DURATION", $"{_configs.SecretCodeValidityDuration} {_configs.SecretCodeValidityDurationUnit}s");
            var smsBinding = new SingleSmsBinding {
                SmsContent = smsContent,
                Receivers = new List<string> { RegionalizedPhoneNumber.Convert(destination).ToString() },
            };
            
            var smsResult = await _clickatellSmsHttpService.SendSingleSms(smsBinding);
            if (smsResult is null || smsResult.Any()) {
                await _contextService.RevertTransaction();
                return new ErrorResponse();
            }
        }
        else {
            var mailBinding = new MailBinding {
                ToReceivers = new List<Recipient> { new() { EmailAddress = destination} },
                Title = $"{Constants.ProjectName}: Secret Code",
                Priority = MailPriority.High,
                TemplateName = Enums.EmailTemplate.SecretCodeEmail,
                Placeholders = new Dictionary<string, string> {
                    { "EMAIL_ADDRESS", destination },
                    { "SECRET_CODE", account.SecretCode },
                    { "VALIDITY_DURATION", $"{_configs.SecretCodeValidityDuration} {_configs.SecretCodeValidityDurationUnit}s" },
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


    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpGet("forward-token")]
    public async Task<IActionResult> ForwardToken([FromHeader] string accountId, [FromQuery] int tokenType, [FromQuery] int destination) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(ForwardToken) });

        if (tokenType < 0 || tokenType > Enum.GetNames<Enums.TokenType>().Length)
            return new ErrorResponse(HttpStatusCode.RequestedRangeNotSatisfiable, nameof(tokenType));
        
        if (destination < 0 || destination > Enum.GetNames<Enums.TokenDestination>().Length)
            return new ErrorResponse(HttpStatusCode.RequestedRangeNotSatisfiable, nameof(destination));
        
        var account = await _accountService.GetAccountById(accountId);
        if (account is null) return new ErrorResponse();
        
        var profile = await _profileService.GetProfileByAccountId(accountId);
        if (profile is null) return new ErrorResponse();

        var tokenData = tokenType switch {
            (byte)Enums.TokenType.AccountRecovery =>  new Tuple<string?, DateTime>(account.RecoveryToken, account.RecoveryTokenTimestamp!.Value),
            (byte)Enums.TokenType.OneTimePassword => new Tuple<string?, DateTime>(account.OneTimePassword, account.OneTimePasswordTimestamp!.Value),
            _ => default,
        };
        
        if (tokenData is null) return new ErrorResponse();
        var (token, tokenTimestamp) = tokenData;
        
        // Only tokens for Account Recovery and One Time Password can be forwarded, other tokens should be renewed
        DateTime? tokenElapsingTime = tokenType switch {
            (byte)Enums.TokenType.AccountRecovery => tokenTimestamp.Compute(_configs.RecoveryTokenValidityDuration, _configs.RecoveryTokenValidityDurationUnit),
            (byte)Enums.TokenType.OneTimePassword => tokenTimestamp.Compute(_configs.OtpValidityDuration, _configs.OtpValidityDurationUnit),
            _ => null,
        };
        if (!tokenElapsingTime.HasValue) return new ErrorResponse();
        if (tokenElapsingTime > DateTime.UtcNow) return new ErrorResponse(HttpStatusCode.RequestedRangeNotSatisfiable, nameof(tokenElapsingTime));

        var tokenSendingExpression = destination switch {
            (byte)Enums.TokenDestination.Email => (Func<Task<bool?>>)(async () => {
                var emailExpiryTimestamp = tokenElapsingTime.Value.Format(Enums.DateFormat.DDMMYYYYS, Enums.TimeFormat.HHMMTTC);
                
                var mailBinding = tokenType switch {
                    (byte)Enums.TokenType.AccountRecovery => new MailBinding {
                        ToReceivers = new List<Recipient> { new() { EmailAddress = account.EmailAddress! } },
                        Title = $"{Constants.ProjectName}: Recover your account",
                        Priority = MailPriority.High,
                        TemplateName = Enums.EmailTemplate.AccountRecoveryEmail,
                        Placeholders = new Dictionary<string, string> {
                            { "EMAIL_ADDRESS", account.EmailAddress! },
                            { "RECOVERY_TOKEN", Uri.EscapeDataString(token!) },
                            { "VALID_UNTIL_DATETIME", emailExpiryTimestamp },
                        },
                    },
                    (byte)Enums.TokenType.OneTimePassword => new MailBinding {
                        ToReceivers = new List<Recipient> { new() { EmailAddress = account.EmailAddress! } },
                        Title = $"{Constants.ProjectName}: Your OTP",
                        Priority = MailPriority.High,
                        TemplateName = Enums.EmailTemplate.OneTimePasswordEmail,
                        Placeholders = new Dictionary<string, string> {
                            { "EMAIL_ADDRESS", account.EmailAddress! },
                            { "ONE_TIME_PASSWORD", Uri.EscapeDataString(token!) },
                            { "VALIDITY_DURATION", _configs.OtpValidityDuration.ToString() },
                            { "VALIDITY_UNIT", _configs.OtpValidityDurationUnit.GetValue()! },
                        },
                    },
                    _ => null,
                };

                if (mailBinding is null) return null;
                return await _mailService.SendSingleEmail(mailBinding);
            }),
            _ => async () => {
                var smsContent = destination switch {
                    (byte)Enums.TokenType.AccountRecovery => _configs.AccountRecoverySmsContent
                                                             .Replace("ACCOUNT_RECOVERY_TOKEN", token)
                                                             .Replace("VALIDITY_DURATION", $"{_configs.RecoveryTokenValidityDuration} {_configs.RecoveryTokenValidityDurationUnit}"),
                    (byte)Enums.TokenType.OneTimePassword => _configs.OneTimePasswordSmsContent
                                                             .Replace("ONE_TIME_PASSWORD", token)
                                                             .Replace("VALIDITY_DURATION", $"{_configs.OtpValidityDuration} {_configs.OtpValidityDurationUnit.GetValue()}"),
                    _ => null,
                };

                if (!smsContent.IsString()) return null;
                if (!profile.PhoneNumber.IsString()) return null;
                
                var smsBinding = new SingleSmsBinding {
                    SmsContent = smsContent!,
                    Receivers = new List<string> { profile.PhoneNumber! },
                };
                
                var smsResult = await _clickatellSmsHttpService.SendSingleSms(smsBinding);
                if (smsResult is null || smsResult.Any()) return null;
                return true;
            }
        };

        var result = tokenSendingExpression.Invoke().Result;
        
        if (!result.HasValue) return new ErrorResponse();
        return !result.Value ? new ErrorResponse(HttpStatusCode.NotImplemented) : new SuccessResponse();
    }

    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPut("renew-token/{currentToken}")]
    public async Task<IActionResult> RenewRegistrationToken([FromHeader] string accountId, [FromRoute] string currentToken, [FromQuery] int tokenType) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(RenewRegistrationToken) });
        
        if (tokenType < 0 || tokenType > Enum.GetNames<Enums.TokenType>().Length)
            return new ErrorResponse(HttpStatusCode.RequestedRangeNotSatisfiable, nameof(tokenType));
        
        var account = await _accountService.GetAccountById(accountId);
        if (account is null) return new ErrorResponse();
        
        var profile = await _profileService.GetProfileByAccountId(accountId);
        if (profile is null) return new ErrorResponse();

        var tokenData = tokenType switch {
            (byte)Enums.TokenType.EmailRegistration => new Tuple<string, DateTime>(account.EmailAddressToken!, account.EmailAddressTokenTimestamp!.Value),
            (byte)Enums.TokenType.PhoneRegistration => new Tuple<string, DateTime>(profile.PhoneNumberToken!, profile.PhoneNumberTokenTimestamp!.Value),
            _ => default,
        };
        if (tokenData is null) return new ErrorResponse();

        var (token, tokenTimestamp) = tokenData;
        if (!Equals(token, currentToken)) return new ErrorResponse(HttpStatusCode.Conflict, nameof(currentToken));
        
        DateTime? tokenElapsingTime = tokenType switch {
            (byte)Enums.TokenType.EmailRegistration => tokenTimestamp.Compute(_configs.EmailTokenValidityDuration, _configs.EmailTokenValidityDurationUnit),
            (byte)Enums.TokenType.PhoneRegistration => tokenTimestamp.Compute(_configs.PhoneTokenValidityDuration, _configs.PhoneTokenValidityDurationUnit),
            _ => null,
        };
        if (!tokenElapsingTime.HasValue) return new ErrorResponse();
        if (tokenElapsingTime > DateTime.UtcNow) return new ErrorResponse(HttpStatusCode.RequestedRangeNotSatisfiable, nameof(tokenElapsingTime));

        token = tokenType switch {
            (byte)Enums.TokenType.EmailRegistration => StringHelpers.GenerateRandomString(GetTokenLengthsForNewAccount().Item1, true),
            (byte)Enums.TokenType.PhoneRegistration => StringHelpers.GenerateRandomString(NumberHelpers.GetRandomNumberInRangeInclusive(_configs.PhoneTokenMinLength, _configs.PhoneTokenMaxLength), true),
            _ => default,
        };

        if (!token.IsString()) return new ErrorResponse();

        var newTokenTimestamp = DateTime.UtcNow;
        var updateTokenExpression = tokenType switch {
            (byte)Enums.TokenType.EmailRegistration => (Func<Task<bool?>>)(async () => {
                account.EmailAddressToken = token;
                account.EmailAddressTokenTimestamp = newTokenTimestamp;
                return await _accountService.UpdateAccount(account);
            }),
            (byte)Enums.TokenType.PhoneRegistration => async () => {
                profile.PhoneNumberToken = token;
                profile.PhoneNumberTokenTimestamp = newTokenTimestamp;
                return await _profileService.UpdateProfile(profile);
            },
            _ => default,
        };

        await _contextService.StartTransaction();
        var tokenUpdateResult = updateTokenExpression?.Invoke().Result;
        if (!tokenUpdateResult.HasValue || !tokenUpdateResult.Value) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        var tokenSendingExpression = tokenType switch {
            (byte)Enums.TokenType.EmailRegistration => (Func<Task<bool?>>)(async () => {
                var newTokenExpiry = newTokenTimestamp.Compute(_configs.EmailTokenValidityDuration, _configs.EmailTokenValidityDurationUnit).Format(Enums.DateFormat.DDMMYYYYS, Enums.TimeFormat.HHMMTTC);
                
                var mailBinding = new MailBinding {
                    ToReceivers = new List<Recipient> { new() { EmailAddress = account.EmailAddress! } },
                    Title = $"{Constants.ProjectName}: Activate your account",
                    Priority = MailPriority.High,
                    TemplateName = Enums.EmailTemplate.AccountActivationEmail,
                    Placeholders = new Dictionary<string, string> {
                        { "EMAIL_ADDRESS", account.EmailAddress! },
                        { "REGISTRATION_TOKEN", Uri.EscapeDataString(token!) },
                        { "USER_NAME", account.Username is null ? string.Empty : Uri.EscapeDataString(account.Username)},
                        { "VALID_UNTIL_DATETIME", newTokenExpiry },
                    },
                };

                return await _mailService.SendSingleEmail(mailBinding);
            }),
            (byte)Enums.TokenType.PhoneRegistration => async () => {
                var smsContent = _configs.AccountActivationSmsContent
                        .Replace("CLIENT_BASE_URI", _configs.ClientBaseUri)
                        .Replace("PHONE_NUMBER", JsonConvert.DeserializeObject<RegionalizedPhoneNumber>(profile.PhoneNumber!)!.Simplify())
                        .Replace("REGISTRATION_TOKEN", Uri.EscapeDataString(profile.PhoneNumberToken!))
                        .Replace("USER_NAME", account.Username is null ? string.Empty : Uri.EscapeDataString(account.Username))
                        .Replace("VALIDITY_DURATION", $"{_configs.PhoneTokenValidityDuration} {_configs.PhoneTokenValidityDurationUnit}s");

                var phoneNumber = JsonConvert.DeserializeObject<RegionalizedPhoneNumber>(profile.PhoneNumber!);
                var smsBinding = new SingleSmsBinding {
                    SmsContent = smsContent,
                    Receivers = new List<string> { phoneNumber!.ToString() },
                };

                var smsResult = await _clickatellSmsHttpService.SendSingleSms(smsBinding);
                return !smsResult?.Any();
            },
            _ => null,
        };

        var tokenSendingResult = tokenSendingExpression?.Invoke().Result;
        if (!tokenSendingResult.HasValue) return new ErrorResponse();
        if (!tokenSendingResult.Value) return new ErrorResponse(HttpStatusCode.NotImplemented);

        await _contextService.ConfirmTransaction();
        return new SuccessResponse();
    }

    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPut("activate-account/{token}")]
    public async Task<ActionResult> ActivateAccount([FromQuery] string token, [FromQuery] int tokenType, [FromQuery] string secretCode, [FromRoute] string medium) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(ActivateAccount) });
        
        if (tokenType < 0 || tokenType > Enum.GetNames<Enums.TokenType>().Length)
            return new ErrorResponse(HttpStatusCode.RequestedRangeNotSatisfiable);
        
        if (!medium.IsString()) return new ErrorResponse(HttpStatusCode.BadRequest);
        
        var isPhoneNumber = new Regex(@"^[\d]{2,2},[\d]{9,9}$").IsMatch(medium);
        var account = await (isPhoneNumber ? _profileService.GetAccountByPhoneNumber(RegionalizedPhoneNumber.Convert(medium)) : _accountService.GetAccountByEmailAddress(medium));
        
        if (account is null) return new ErrorResponse();

        Profile? profile = null;
        if (isPhoneNumber) {
            profile = await _profileService.GetProfileByPhoneNumber(RegionalizedPhoneNumber.Convert(medium));
            if (profile is null) return new ErrorResponse();
        }

        
        if (!Equals(account.SecretCode, secretCode)) return new ErrorResponse(HttpStatusCode.Conflict, new { data = nameof(secretCode) });
        if (account.SecretCodeTimestamp.HasValue && account.SecretCodeTimestamp.Value.Compute(_configs.SecretCodeValidityDuration, _configs.SecretCodeValidityDurationUnit) < DateTime.UtcNow)
            return new ErrorResponse(HttpStatusCode.Gone, new { data = nameof(secretCode) });

        var activationExpression = tokenType switch {
            (byte)Enums.TokenType.EmailRegistration => (Func<Task<bool?>>)(async () => {
                if (!Equals(account.EmailAddressToken, token)) return null;

                var tokenElapsingTime = account.EmailAddressTokenTimestamp!.Value.Compute(_configs.EmailTokenValidityDuration, _configs.EmailTokenValidityDurationUnit);
                if (tokenElapsingTime > DateTime.UtcNow) return false;

                account.EmailAddressToken = null;
                account.EmailAddressTokenTimestamp = null;
                account.EmailConfirmed = true;

                return await _accountService.UpdateAccount(account);
            }),
            (byte)Enums.TokenType.PhoneRegistration => async () => {
                if (!Equals(profile!.PhoneNumberToken, token)) return null;

                var tokenElapsingTime = profile.PhoneNumberTokenTimestamp!.Value.Compute(_configs.PhoneTokenValidityDuration, _configs.PhoneTokenValidityDurationUnit);
                if (tokenElapsingTime > DateTime.UtcNow) return false;

                profile.PhoneNumberToken = null;
                profile.PhoneNumberTokenTimestamp = null;
                profile.PhoneNumberConfirmed = true;

                return await _profileService.UpdateProfile(profile);
            },
            _ => null,
        };

        var result = activationExpression?.Invoke().Result;
        if (!result.HasValue) return new ErrorResponse(HttpStatusCode.Conflict, new { data = nameof(token) });

        return !result.Value ? new ErrorResponse(HttpStatusCode.Gone, new { data = nameof(token) }) : new SuccessResponse();
    }

    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPost("authenticate-by-credentials")]
    public async Task<IActionResult> AuthenticateByCredentials([FromBody] AuthenticationData authenticationData) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(AuthenticateByCredentials) });
        if (_httpContext is null) return new ErrorResponse();

        var errors = await authenticationData.VerifyAuthenticationData(_phoneNumberHandler);
        if (errors.Any()) return new ErrorResponse(HttpStatusCode.BadRequest, errors);
        
        var (account, profile) = await GetAccountAndProfileByLoginInformation(new LoginInformation { EmailAddress = authenticationData.EmailAddress, PhoneNumber = authenticationData.PhoneNumber });
        
        var authenticateByEmailAddress = authenticationData.EmailAddress.IsString();
        var dataError = VerifyAccountAndProfileData(account, profile, authenticateByEmailAddress);
        if (dataError is not null) return dataError;

        // Todo: implement trusted device authorization

        if (
            (
                account!.LockOutEnabled &&
                account.LockOutOn!.Value.Compute(_configs.LockOutDuration, _configs.LockOutDurationUnit) <= DateTime.UtcNow
            ) || account.IsSuspended
        ) return new ErrorResponse(HttpStatusCode.Locked);
        
        var isPasswordMatched = _cryptoService.IsHashMatchesPlainText(account.HashPassword, authenticationData.Password);

        if (!isPasswordMatched) {
            var result = await UpdateLockoutAndSuspendOnFailedLogin(account);
            return !result.HasValue || !result.Value
                ? new ErrorResponse()
                : new ErrorResponse(HttpStatusCode.Conflict, nameof(isPasswordMatched));
        }

        var authenticatedUser = await CreateAuthenticatedUser(account, profile!);
        return authenticatedUser is null ? new ErrorResponse() : new SuccessResponse(authenticatedUser);
    }

    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPost("authenticate-by-otp")]
    public async Task<IActionResult> AuthenticateByOneTimePassword([FromBody] LoginInformation loginInformation) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(AuthenticateByOneTimePassword) });
        if (_httpContext is null) return new ErrorResponse();

        var errors = await loginInformation.VerifyLoginInformation(_phoneNumberHandler);
        if (errors.Any()) return new ErrorResponse(HttpStatusCode.BadRequest, errors);

        var (account, profile) = await GetAccountAndProfileByLoginInformation(new LoginInformation { EmailAddress = loginInformation.EmailAddress, PhoneNumber = loginInformation.PhoneNumber });
        if (
            (
                account!.LockOutEnabled &&
                account.LockOutOn!.Value.Compute(_configs.LockOutDuration, _configs.LockOutDurationUnit) <= DateTime.UtcNow
            ) || account.IsSuspended
        ) return new ErrorResponse(HttpStatusCode.Locked);
        
        var authenticateByEmailAddress = loginInformation.EmailAddress.IsString();
        var dataError = VerifyAccountAndProfileData(account, profile, authenticateByEmailAddress);
        if (dataError is not null) return dataError;

        account.OneTimePassword = StringHelpers.GenerateRandomString(NumberHelpers.GetRandomNumberInRangeInclusive(_configs.OtpMinLength, _configs.OtpMaxLength));
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
                ToReceivers = new List<Recipient> { new() { EmailAddress = account.EmailAddress! } },
                Title = $"{Constants.ProjectName}: Your OTP",
                Priority = MailPriority.High,
                TemplateName = Enums.EmailTemplate.OneTimePasswordEmail,
                Placeholders = new Dictionary<string, string> {
                    { "EMAIL_ADDRESS", account.EmailAddress! },
                    { "ONE_TIME_PASSWORD", Uri.EscapeDataString(account.OneTimePassword) },
                    { "VALIDITY_DURATION", _configs.OtpValidityDuration.ToString() },
                    { "VALIDITY_UNIT", _configs.OtpValidityDurationUnit.GetValue()! }
                }
            };

            var emailSendingResult = await _mailService.SendSingleEmail(mailBinding);
            if (!emailSendingResult) {
                await _contextService.RevertTransaction();
                return new ErrorResponse();
            }
        }
        else {
            var smsContent = _configs.OneTimePasswordSmsContent
                             .Replace("ONE_TIME_PASSWORD", account.OneTimePassword)
                             .Replace("VALIDITY_DURATION", $"{_configs.OtpValidityDuration} {_configs.OtpValidityDurationUnit.GetValue()}");

            var phoneNumber = JsonConvert.DeserializeObject<RegionalizedPhoneNumber>(profile!.PhoneNumber!);
            var smsBinding = new SingleSmsBinding {
                SmsContent = smsContent,
                Receivers = new List<string> { phoneNumber!.ToString() }
            };

            var smsResult = await _clickatellSmsHttpService.SendSingleSms(smsBinding);
            if (smsResult is null || smsResult.Any()) {
                await _contextService.RevertTransaction();
                return new ErrorResponse();
            }
        }

        var preAuthenticatedUser = new Authorization {
            AccountId = account.Id,
            AuthorizationToken = await _cryptoService.CreateSha512Hash(StringHelpers.GenerateRandomString(Constants.RandomStringDefaultLength, true)),
            AuthorizedTimestamp = account.OneTimePasswordTimestamp.Value.ToTimestamp()
        };
        _httpContext.Session.SetString($"{nameof(preAuthenticatedUser)}{Constants.Underscore}{account.Id}", JsonConvert.SerializeObject(preAuthenticatedUser));

        await _contextService.ConfirmTransaction();
        return new SuccessResponse(preAuthenticatedUser);
    }

    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPost("verify-otp/{oneTimePassword}")]
    public async Task<IActionResult> VerifyOneTimePassword([FromHeader] string accountId, [FromHeader] string authenticationToken, [FromRoute] string oneTimePassword) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(VerifyOneTimePassword) });
        if (_httpContext is null) return new ErrorResponse();

        var sessionPreAuthenticatedUser = _httpContext.Session.GetString($"preAuthenticatedUser{Constants.Underscore}{accountId}");
        if (!sessionPreAuthenticatedUser.IsString()) return new ErrorResponse(HttpStatusCode.Gone);
        
        var preAuthenticatedUser = JsonConvert.DeserializeObject<Authorization>(sessionPreAuthenticatedUser!);
        if (preAuthenticatedUser is null) return new ErrorResponse();
        
        var account = await _accountService.GetAccountById(accountId);
        if (account is null) return new ErrorResponse();

        _httpContext.Session.Remove($"{nameof(preAuthenticatedUser)}{Constants.Underscore}{account.Id}");
        
        if (
            (
                account.LockOutEnabled &&
                account.LockOutOn!.Value.Compute(_configs.LockOutDuration, _configs.LockOutDurationUnit) <= DateTime.UtcNow
            ) || account.IsSuspended
        ) return new ErrorResponse(HttpStatusCode.Locked);

        var isOneTimePasswordMatchedAndValid =
            Equals(account.OneTimePassword, oneTimePassword) &&
            preAuthenticatedUser.AuthorizedTimestamp + _configs.OtpValidityDuration.ToMilliseconds(_configs.OtpValidityDurationUnit) < DateTime.UtcNow.ToTimestamp();

        if (!isOneTimePasswordMatchedAndValid || !Equals(authenticationToken, preAuthenticatedUser.AuthorizationToken)) {
            var result = await UpdateLockoutAndSuspendOnFailedLogin(account);
            return !result.HasValue || !result.Value
                ? new ErrorResponse()
                : new ErrorResponse(HttpStatusCode.Conflict);
        }

        var profile = await _profileService.GetProfileByAccountId(accountId);
        if (profile is null) return new ErrorResponse();
        
        var authenticatedUser = await CreateAuthenticatedUser(account, profile);
        return authenticatedUser is null
            ? new ErrorResponse()
            : new SuccessResponse(authenticatedUser);
    }

    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPost("authenticate-by-cookies")]
    public async Task<IActionResult> AuthenticateByCookies() {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(AuthenticateByCookies) });
        if (_httpContext is null) return new JsonResult(new StatusCodeResult((int)HttpStatusCode.InternalServerError));

        throw new NotImplementedException();
    }

    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] LoginInformation loginInformation) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(ForgotPassword) });
        
        var errors = await loginInformation.VerifyLoginInformation(_phoneNumberHandler);
        if (errors.Any()) return new ErrorResponse(HttpStatusCode.BadRequest, errors);

        var relyOnAccount = loginInformation.EmailAddress.IsString();
        var (account, profile) = await GetAccountAndProfileByLoginInformation(loginInformation, !relyOnAccount);
        
        if (account is null || (!relyOnAccount && profile is null)) return new ErrorResponse();
        
        var shouldProcessForgotPassword = ((relyOnAccount && account.EmailConfirmed) || (!relyOnAccount && profile!.PhoneNumberConfirmed)) && !account.IsSuspended;
        if (!shouldProcessForgotPassword) return new ErrorResponse(HttpStatusCode.Locked);

        account.RecoveryToken = StringHelpers.GenerateRandomString(NumberHelpers.GetRandomNumberInRangeInclusive(_configs.RecoveryTokenMinLength, _configs.RecoveryTokenMaxLength), true);
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
                                       .Compute(_configs.RecoveryTokenValidityDuration, _configs.RecoveryTokenValidityDurationUnit)
                                       .Format(Enums.DateFormat.DDMMYYYYS, Enums.TimeFormat.HHMMTTC);
            
            var emailBinding = new MailBinding {
                ToReceivers = new List<Recipient> { new() { EmailAddress = account.EmailAddress! } },
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
            var smsContent = _configs.AccountRecoverySmsContent
                             .Replace("ACCOUNT_RECOVERY_TOKEN", account.RecoveryToken)
                             .Replace("VALIDITY_DURATION", $"{_configs.RecoveryTokenValidityDuration} {_configs.RecoveryTokenValidityDurationUnit}");
            
            var smsBinding = new SingleSmsBinding {
                SmsContent = smsContent,
                Receivers = new List<string> { profile!.PhoneNumber! }
            };

            var smsSendingResult = await _clickatellSmsHttpService.SendSingleSms(smsBinding);
            tokenSendingResult = smsSendingResult is not null && !smsSendingResult.Any();
        }

        if (!tokenSendingResult) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        await _contextService.ConfirmTransaction();
        return new SuccessResponse();
    }

    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPatch("recover-account/{recoveryToken}")]
    public async Task<IActionResult> RecoverAccount([FromBody] RegistrationData registrationData, [FromRoute] string recoveryToken) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(RecoverAccount) });
        
        var errors = await registrationData.VerifyRegistrationData(_phoneNumberHandler);
        if (errors.Any()) return new ErrorResponse(HttpStatusCode.BadRequest, errors);
        
        var relyOnAccount = registrationData.EmailAddress.IsString();
        var (account, profile) = await GetAccountAndProfileByLoginInformation(registrationData, !relyOnAccount);
        
        if (account is null || (!relyOnAccount && profile is null)) return new ErrorResponse();
        
        var shouldRecoverAccount = ((relyOnAccount && account.EmailConfirmed) || (!relyOnAccount && profile!.PhoneNumberConfirmed)) && !account.IsSuspended;
        if (!shouldRecoverAccount) return new ErrorResponse(HttpStatusCode.Locked);

        if (
            !Equals(account.RecoveryToken, Uri.UnescapeDataString(recoveryToken)) ||
            account.RecoveryTokenTimestamp!.Value.Compute(_configs.RecoveryTokenValidityDuration, _configs.RecoveryTokenValidityDurationUnit) > DateTime.UtcNow
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

    [ServiceFilter(typeof(AuthenticatedAuthorize))]
    [ServiceFilter(typeof(RecaptchaAuthorize))]
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

        var twoFactorSecretKey = StringHelpers.GenerateRandomString(NumberHelpers.GetRandomNumberInRangeInclusive(_configs.TfaKeyMinLength, _configs.TfaKeyMaxLength));
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
                verifyingTokens = twoFactorVerifyingTokens
            });
    }
    
    [ServiceFilter(typeof(AuthenticatedAuthorize))]
    [ServiceFilter(typeof(RecaptchaAuthorize))]
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
        
        if (authenticatedByEmail && !account.EmailConfirmed)
            return new ErrorResponse(HttpStatusCode.Locked);
        
        return !profile.PhoneNumberConfirmed
            ? new ErrorResponse(HttpStatusCode.Locked)
            : default;
    }

    private async Task<bool?> UpdateLockoutAndSuspendOnFailedLogin(Account account) {
        bool? result;
            
        if (++account.LoginFailedCount < _configs.LoginFailedThreshold) {
            account.LockOutEnabled = false;
            account.LockOutOn = null;
            result = await _accountService.UpdateAccount(account);
        }
        else {
            account.LockOutEnabled = true;
            account.LockOutOn = DateTime.UtcNow;

            if (++account.LockOutCount < _configs.LockOutThreshold) {
                account.LoginFailedCount = 0;
            }
            else account.IsSuspended = true;
                
            result = await _accountService.UpdateAccount(account);
        }

        if (account.LockOutEnabled || account.IsSuspended) _httpContext!.Session.Clear();
        return result;
    }

    private async Task<Authorization?> CreateAuthenticatedUser(Account account, Profile profile) {
        var roles = await _roleService.GetAllAccountRoles(account.Id);
        if (roles is null) return null;
        
        var jwtToken = _jwtService.GenerateRequestAuthenticationToken(new Dictionary<string, string> {
            { ClaimTypes.Actor, account.Id },
            { ClaimTypes.Authentication, true.ToString() },
            { ClaimTypes.Role, string.Join(Constants.Comma, roles) },
            { ClaimTypes.Email, account.EmailAddress ?? string.Empty },
            { ClaimTypes.Gender, profile.Gender.ToString() },
            { ClaimTypes.Name, $"{profile.GivenName}{profile.MiddleName}{profile.LastName}" },
            { ClaimTypes.MobilePhone, profile.PhoneNumber ?? string.Empty },
            { ClaimTypes.SerialNumber, account.UniqueCode },
            { ClaimTypes.DateOfBirth, profile.DateOfBirth.HasValue ? profile.DateOfBirth.Value.ToString(CultureInfo.InvariantCulture) : string.Empty }
        });

        var authenticatedTimestamp = DateTimeOffset.UtcNow.Millisecond;
        var authenticatedUser = new Authorization {
            AccountId = account.Id,
            Roles = roles,
            AuthorizationToken = await _cryptoService.CreateSha512Hash(StringHelpers.GenerateRandomString(Constants.RandomStringDefaultLength, true)),
            AuthorizedTimestamp = authenticatedTimestamp,
            BearerToken = jwtToken,
            RefreshToken = await _cryptoService.CreateSha512Hash(StringHelpers.GenerateRandomString(Constants.RandomStringDefaultLength, true)),
            ValidityDuration = _configs.AuthenticationValidityDuration.ToMilliseconds(_configs.AuthenticationValidityDurationUnit)
        };
        
        Response.Cookies.Append(nameof(Authorization.AuthorizedTimestamp), authenticatedTimestamp.ToString(), _cookieOptions);
        Response.Cookies.Append(nameof(Authorization.RefreshToken), authenticatedUser.RefreshToken, _cookieOptions);
        _httpContext!.Session.SetString($"{nameof(Authorization)}{Constants.Underscore}{account.Id}", JsonConvert.SerializeObject(authenticatedUser));

        return authenticatedUser;
    }

    private Tuple<int, int> GetTokenLengthsForNewAccount(bool forSaltOnly = false) {
        var saltLength = NumberHelpers.GetRandomNumberInRangeInclusive(_configs.SaltMinLength, _configs.SaltMaxLength);
        if (forSaltOnly) return new Tuple<int, int>(0, saltLength);
        
        var verificationTokenLength = NumberHelpers.GetRandomNumberInRangeInclusive(_configs.EmailTokenMinLength, _configs.EmailTokenMaxLength);
        return new Tuple<int, int>(verificationTokenLength, saltLength);
    }
}
