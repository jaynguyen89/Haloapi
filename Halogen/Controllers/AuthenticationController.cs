using System.Net;
using System.Net.Mail;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Interfaces.IServiceFactory;
using Halogen.Bindings.ApiBindings;
using Halogen.Parsers;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Mvc;
using Halogen.Attributes;
using Halogen.Bindings.DataMappers;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary;
using Newtonsoft.Json;

namespace Halogen.Controllers; 

[ApiController]
[Route("authentication")]
public sealed class AuthenticationController: AppController {

    private readonly IContextService _contextService;
    private readonly IAuthenticationService _authenticationService;
    private readonly ICryptoService _cryptoService;
    private readonly IMailService _mailService;
    private readonly IAccountService _accountService;
    private readonly IProfileService _profileService;
    private readonly IRoleService _roleService;
    private readonly IPreferenceService _preferenceService;
    private readonly ISmsService _clickatellSmsHttpService;
    private readonly IAssistantService _assistantService;
    private readonly ITwoFactorService _twoFactorService;

    private readonly int _saltMinLength;
    private readonly int _saltMaxLength;
    private readonly int _tfaKeyMinLength;
    private readonly int _tfaKeyMaxLength;
    private readonly int _emailTokenMinLength;
    private readonly int _emailTokenMaxLength;
    private readonly int _emailTokenValidityDuration;
    private readonly Enums.TimeUnit _emailTokenValidityDurationUnit;
    private readonly int _otpMinLength;
    private readonly int _otpMaxLength;
    private readonly int _otpValidityDuration;
    private readonly Enums.TimeUnit _otpValidityDurationUnit;
    private readonly int _recoveryTokenMinLength;
    private readonly int _recoveryTokenMaxLength;
    private readonly int _recoveryTokenValidityDuration;
    private readonly Enums.TimeUnit _recoveryTokenValidityDurationUnit;
    private readonly int _phoneTokenMinLength;
    private readonly int _phoneTokenMaxLength;
    private readonly int _phoneTokenValidityDuration;
    private readonly Enums.TimeUnit _phoneTokenValidityDurationUnit;
    private readonly int _loginFailedThreshold;
    private readonly int _lockOutThreshold;
    private readonly int _lockOutDuration;
    private readonly Enums.TimeUnit _lockOutDurationUnit;
    private readonly string _accountActivationSmsContent;
    private readonly string _accountRecoverySmsContent;
    private readonly string _twoFactorPinSmsContent;
    private readonly string _oneTimePasswordSmsContent;

    public AuthenticationController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        IContextService contextService,
        IAuthenticationService authenticationService,
        ICryptoService cryptoService,
        IMailService mailService,
        IAccountService accountService,
        IProfileService profileService,
        IRoleService roleService,
        IPreferenceService preferenceService,
        IAssistantService assistantService,
        ISmsServiceFactory smsServiceFactory,
        ITwoFactorService twoFactorService
    ) : base(ecosystem, logger, configuration) {
        _contextService = contextService;
        _authenticationService = authenticationService;
        _cryptoService = cryptoService;
        _mailService = mailService;
        _accountService = accountService;
        _profileService = profileService;
        _roleService = roleService;
        _preferenceService = preferenceService;
        _assistantService = assistantService;
        _clickatellSmsHttpService = smsServiceFactory.GetActiveSmsService();
        _twoFactorService = twoFactorService;
        ParseSecuritySettings(
            out _saltMinLength, out _saltMaxLength, out _tfaKeyMinLength, out _tfaKeyMaxLength,
            out _emailTokenMinLength, out _emailTokenMaxLength, out _emailTokenValidityDuration, out _emailTokenValidityDurationUnit,
            out _otpMinLength, out _otpMaxLength, out _otpValidityDuration, out _otpValidityDurationUnit,
            out _recoveryTokenMinLength, out _recoveryTokenMaxLength, out _recoveryTokenValidityDuration, out _recoveryTokenValidityDurationUnit,
            out _phoneTokenMinLength, out _phoneTokenMaxLength, out _phoneTokenValidityDuration, out _phoneTokenValidityDurationUnit,
            out _loginFailedThreshold, out _lockOutThreshold, out _lockOutDuration, out _lockOutDurationUnit,
            out _accountActivationSmsContent, out _accountRecoverySmsContent, out _twoFactorPinSmsContent, out _oneTimePasswordSmsContent
        );
    }

    private void ParseSecuritySettings(
        out int saltMinLength, out int saltMaxLength, out int tfaKeyMinLength, out int tfaKeyMaxLength,
        out int emailTokenMinLength, out int emailTokenMaxLength, out int emailTokenValidityDuration, out Enums.TimeUnit emailTokenValidityDurationUnit,
        out int otpMinLength, out int otpMaxLength, out int otpValidityDuration, out Enums.TimeUnit otpValidityDurationUnit,
        out int recoveryTokenMinLength, out int recoveryTokenMaxLength, out int recoveryTokenValidityDuration, out Enums.TimeUnit recoveryTokenValidityDurationUnit,
        out int phoneTokenMinLength, out int phoneTokenMaxLength, out int phoneTokenValidityDuration, out Enums.TimeUnit phoneTokenValidityDurationUnit,
        out int loginFailedThreshold, out int lockOutThreshold, out int lockOutDuration, out Enums.TimeUnit lockOutDurationUnit,
        out string accountActivationSmsContent, out string accountRecoverySmsContent, out string twoFactorPinSmsContent, out string oneTimePasswordSmsContent
    ) {
        (
            saltMinLength, saltMaxLength, tfaKeyMinLength, tfaKeyMaxLength,
            emailTokenMinLength, emailTokenMaxLength, emailTokenValidityDuration, emailTokenValidityDurationUnit
        ) = (
            int.Parse(_configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.SaltMinLength)}")),
            int.Parse(_configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.SaltMaxLength)}")),
            int.Parse(_configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.TfaKeyMinLength)}")),
            int.Parse(_configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.TfaKeyMaxLength)}")),
            int.Parse(_configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.EmailTokenMinLength)}")),
            int.Parse(_configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.EmailTokenMaxLength)}")),
            int.Parse(_configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.EmailTokenValidityDuration)}")),
            _configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.EmailTokenValidityDurationUnit)}").ToEnum(Enums.TimeUnit.Hour)
        );
        
        (
            otpMinLength, otpMaxLength, otpValidityDuration, otpValidityDurationUnit,
            recoveryTokenMinLength, recoveryTokenMaxLength, recoveryTokenValidityDuration, recoveryTokenValidityDurationUnit
        ) = (
            int.Parse(_configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.OtpMinLength)}")),
            int.Parse(_configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.OtpMaxLength)}")),
            int.Parse(_configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.OtpValidityDuration)}")),
            _configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.OtpValidityDurationUnit)}").ToEnum(Enums.TimeUnit.Hour),
            int.Parse(_configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.RecoveryTokenMinLength)}")),
            int.Parse(_configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.RecoveryTokenMaxLength)}")),
            int.Parse(_configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.RecoveryTokenValidityDuration)}")),
            _configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.RecoveryTokenValidityDurationUnit)}").ToEnum(Enums.TimeUnit.Hour)
        );
        
        (
            phoneTokenMinLength, phoneTokenMaxLength, phoneTokenValidityDuration, phoneTokenValidityDurationUnit,
            loginFailedThreshold, lockOutThreshold, lockOutDuration, lockOutDurationUnit
        ) = (
            int.Parse(_configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.PhoneTokenMinLength)}")),
            int.Parse(_configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.PhoneTokenMaxLength)}")),
            int.Parse(_configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.PhoneTokenValidityDuration)}")),
            _configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.PhoneTokenValidityDurationUnit)}").ToEnum(Enums.TimeUnit.Hour),
            int.Parse(_configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.LoginFailedThreshold)}")),
            int.Parse(_configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.LockOutThreshold)}")),
            int.Parse(_configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.LockOutDuration)}")),
            _configuration.GetValue<string>($"{_baseSecuritySettingsOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.LockOutDurationUnit)}").ToEnum(Enums.TimeUnit.Hour)
        );
        
        (accountActivationSmsContent, accountRecoverySmsContent, twoFactorPinSmsContent, oneTimePasswordSmsContent) = (
            _configuration.GetValue<string>($"{_smsContentsOptionKey}{nameof(HalogenOptions.Local.SmsContents.AccountActivationSms)}"),
            _configuration.GetValue<string>($"{_smsContentsOptionKey}{nameof(HalogenOptions.Local.SmsContents.AccountRecoverySms)}"),
            _configuration.GetValue<string>($"{_smsContentsOptionKey}{nameof(HalogenOptions.Local.SmsContents.TwoFactorPinSms)}"),
            _configuration.GetValue<string>($"{_smsContentsOptionKey}{nameof(HalogenOptions.Local.SmsContents.OneTimePasswordSms)}")
        );
    }

    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPost("register-account")]
    public async Task<JsonResult> RegisterAccount(RegistrationData registrationData) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(RegisterAccount) });

        var errors = await registrationData.VerifyRegistrationData();
        if (errors.Any()) return new JsonResult(new ClientResponse { Result = Enums.ApiResult.Failed, Data = errors });

        var registerByEmailAddress = registrationData.EmailAddress.IsString();
        if (registerByEmailAddress) {
            var isEmailAvailable = await _accountService.IsEmailAddressAvailableForNewAccount(registrationData.EmailAddress!);
            if (!isEmailAvailable.HasValue) return new JsonResult(new StatusCodeResult((int)HttpStatusCode.InternalServerError));
            if (!isEmailAvailable.Value) return new JsonResult(new ClientResponse { Result = Enums.ApiResult.Failed, Data = new { isEmailAvailable } });
        }
        else {
            var isPhoneNumberAvailable = await _profileService.IsPhoneNumberAvailableForNewAccount(registrationData.PhoneNumber!.ToString());
            if (!isPhoneNumberAvailable.HasValue) return new JsonResult(new StatusCodeResult((int)HttpStatusCode.InternalServerError));
            if (!isPhoneNumberAvailable.Value) return new JsonResult(new ClientResponse { Result = Enums.ApiResult.Failed, Data = new { isPhoneNumberAvailable } });
        }

        var (verificationTokenLength, saltLength) = GetTokenLengthsForNewAccount();
        var (hashedPassword, salt) = _cryptoService.GenerateHashAndSalt(registrationData.Password, saltLength);

        var newAccount = Account.CreateNewAccount(_useLongerId, registrationData.EmailAddress, salt, hashedPassword, verificationTokenLength);
        await _contextService.StartTransaction();

        var accountId = await _authenticationService.InsertNewAccount(newAccount);
        if (accountId is null) {
            await _contextService.RevertTransaction();
            return new JsonResult(new StatusCodeResult((int)HttpStatusCode.InternalServerError));
        }
        
        var newProfile = Profile.CreateNewProfile(_useLongerId, accountId, registerByEmailAddress, _phoneTokenMinLength, _phoneTokenMaxLength, registrationData.PhoneNumber);
        
        var profileId = await _profileService.InsertNewProfile(newProfile);
        if (profileId is null) {
            await _contextService.RevertTransaction();
            return new JsonResult(new StatusCodeResult((int)HttpStatusCode.InternalServerError));
        }

        var role = await _roleService.GetRoleByName(Enums.Role.Customer.GetValue()!);
        if (role is null) {
            await _contextService.RevertTransaction();
            return new JsonResult(new StatusCodeResult((int)HttpStatusCode.InternalServerError));
        }

        var accountRoleId = await _roleService.InsertNewAccountRole(new AccountRole {
            Id = StringHelpers.NewGuid(_useLongerId),
            AccountId = accountId,
            RoleId = role.Id,
            IsEffective = true
        });
        if (accountRoleId is null) {
            await _contextService.RevertTransaction();
            return new JsonResult(new StatusCodeResult((int)HttpStatusCode.InternalServerError));
        }

        var defaultPreference = Preference.CreatePreferenceForNewAccount(_useLongerId, accountId);
        var preferenceId = await _preferenceService.InsertNewPreference(defaultPreference);
        if (preferenceId is null) {
            await _contextService.RevertTransaction();
            return new JsonResult(new StatusCodeResult((int)HttpStatusCode.InternalServerError));
        }

        if (registerByEmailAddress) {
            var accountActivationEmailExpiry = newAccount.EmailAddressTokenTimestamp!.Value
                                                         .Compute(_emailTokenValidityDuration, _emailTokenValidityDurationUnit)
                                                         .Format(Enums.DateFormat.DDMMYYYYS, Enums.TimeFormat.HHMMTTC);
            
            var mailBinding = new MailBinding {
                ToReceivers = new List<Recipient> { new() { EmailAddress = newAccount.EmailAddress! } },
                Title = $"{Constants.ProjectName}: Activate your account",
                Priority = MailPriority.High,
                TemplateName = Enums.EmailTemplate.AccountActivationEmail,
                Placeholders = new Dictionary<string, string> {
                    { "EMAIL_ADDRESS", newAccount.EmailAddress! },
                    { "REGISTRATION_TOKEN", newAccount.EmailAddressToken! },
                    { "VALID_UNTIL_DATETIME", accountActivationEmailExpiry }
                }
            };

            var isAccountActivationEmailSent = await _mailService.SendSingleEmail(mailBinding);
            if (!isAccountActivationEmailSent) {
                await _contextService.RevertTransaction();
                return new JsonResult(new StatusCodeResult((int)HttpStatusCode.InternalServerError));
            }
        }
        else {
            var smsContent = _accountActivationSmsContent
                             .Replace("ACCOUNT_ACTIVATION_TOKEN", newProfile.PhoneNumberToken)
                             .Replace("VALIDITY_DURATION", $"{_phoneTokenValidityDuration} {_phoneTokenValidityDurationUnit}");
            var smsBinding = new SingleSmsBinding {
                SmsContent = smsContent,
                Receivers = new List<string> { registrationData.PhoneNumber!.ToString() }
            };

            var smsResult = await _clickatellSmsHttpService.SendSingleSms(smsBinding);
            if (smsResult is null || smsResult.Any()) {
                await _contextService.RevertTransaction();
                return new JsonResult(new StatusCodeResult((int)HttpStatusCode.InternalServerError));
            }
        }

        await _contextService.ConfirmTransaction();
        return new JsonResult(new ClientResponse {
            Result = Enums.ApiResult.Success,
            Data = new {
                accountId,
                registerByEmailAddress,
                phoneTokenValidityDuration = _phoneTokenValidityDuration,
                phoneTokenValidityDurationUnit = _phoneTokenValidityDurationUnit
            }
        });
    }

    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpGet("forward-token/{tokenType:int}/{destination:int}")]
    public async Task<JsonResult> ForwardToken([FromHeader] string accountId, [FromRoute] int tokenType, [FromRoute] int destination) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(ForwardToken) });

        if (tokenType < 0 || destination < 0 || tokenType > Enum.GetNames<Enums.TokenType>().Length || destination > Enum.GetNames<Enums.TokenDestination>().Length)
            return new JsonResult(new StatusCodeResult((int)HttpStatusCode.RequestedRangeNotSatisfiable));
        
        var account = await _accountService.GetAccountById(accountId);
        if (account is null) return new JsonResult(new StatusCodeResult((int)HttpStatusCode.InternalServerError));
        
        var profile = await _profileService.GetProfileByAccountId(accountId);
        if (profile is null) return new JsonResult(new StatusCodeResult((int)HttpStatusCode.InternalServerError));

        var tokenData = tokenType switch {
            (byte)Enums.TokenType.AccountRecovery =>  new Tuple<string, DateTime>(account.RecoveryToken!, account.RecoveryTokenTimestamp!.Value),
            (byte)Enums.TokenType.OneTimePassword => new Tuple<string, DateTime>(account.OneTimePassword!, account.OneTimePasswordTimestamp!.Value),
            _ => default
        };
        
        if (tokenData is null) return new JsonResult(new StatusCodeResult((int)HttpStatusCode.InternalServerError));
        var (token, tokenTimestamp) = tokenData;
        
        // Only tokens for Account Recovery and One Time Password can be forwarded, other tokens should be renewed
        DateTime? tokenElapsingTime = tokenType switch {
            (byte)Enums.TokenType.AccountRecovery => tokenTimestamp.Compute(_recoveryTokenValidityDuration, _recoveryTokenValidityDurationUnit),
            (byte)Enums.TokenType.OneTimePassword => tokenTimestamp.Compute(_otpValidityDuration, _otpValidityDurationUnit),
            _ => null
        };
        if (!tokenElapsingTime.HasValue) return new JsonResult(new StatusCodeResult((int)HttpStatusCode.Forbidden));
        if (tokenElapsingTime > DateTime.UtcNow) return new JsonResult(new StatusCodeResult((int)HttpStatusCode.RequestTimeout));

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
                            { "RECOVERY_TOKEN", token },
                            { "VALID_UNTIL_DATETIME", emailExpiryTimestamp }
                        }
                    },
                    (byte)Enums.TokenType.OneTimePassword => new MailBinding {
                        ToReceivers = new List<Recipient> { new() { EmailAddress = account.EmailAddress! } },
                        Title = $"{Constants.ProjectName}: Activate your account",
                        Priority = MailPriority.High,
                        TemplateName = Enums.EmailTemplate.OneTimePasswordEmail,
                        Placeholders = new Dictionary<string, string> {
                            { "EMAIL_ADDRESS", account.EmailAddress! },
                            { "ONE_TIME_PASSWORD", token },
                            { "VALIDITY_DURATION", _otpValidityDuration.ToString() },
                            { "VALIDITY_UNIT", _otpValidityDurationUnit.GetValue()! }
                        }
                    },
                    _ => null
                };

                if (mailBinding is null) return null;
                return await _mailService.SendSingleEmail(mailBinding);
            }),
            _ => async () => {
                var smsContent = destination switch {
                    (byte)Enums.TokenType.AccountRecovery => "",
                    (byte)Enums.TokenType.OneTimePassword => "",
                    _ => null
                };

                if (!smsContent.IsString()) return null;
                if (!profile.PhoneNumber.IsString()) return null;
                
                var smsBinding = new SingleSmsBinding {
                    SmsContent = smsContent!,
                    Receivers = new List<string> { profile.PhoneNumber! }
                };
                
                var smsResult = await _clickatellSmsHttpService.SendSingleSms(smsBinding);
                if (smsResult is null || smsResult.Any()) return null;
                return true;
            }
        };

        var result = tokenSendingExpression.Invoke().Result;
        return new JsonResult(
            !result.HasValue || !result.Value
                ? new StatusCodeResult((int)HttpStatusCode.InternalServerError)
                : new ClientResponse { Result = Enums.ApiResult.Success }
        );
    }

    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPost("renew-token/{currentToken}/{tokenType:int}")]
    public async Task<JsonResult> RenewRegistrationToken([FromHeader] string accountId, [FromRoute] string currentToken, [FromRoute] int tokenType) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(RenewRegistrationToken) });
        
        if (tokenType < 0 || tokenType > Enum.GetNames<Enums.TokenType>().Length)
            return new JsonResult(new StatusCodeResult((int)HttpStatusCode.RequestedRangeNotSatisfiable));
        
        var account = await _accountService.GetAccountById(accountId);
        if (account is null) return new JsonResult(new StatusCodeResult((int)HttpStatusCode.InternalServerError));
        
        var profile = await _profileService.GetProfileByAccountId(accountId);
        if (profile is null) return new JsonResult(new StatusCodeResult((int)HttpStatusCode.InternalServerError));

        var tokenData = tokenType switch {
            (byte)Enums.TokenType.EmailRegistration => new Tuple<string, DateTime>(account.EmailAddressToken!, account.EmailAddressTokenTimestamp!.Value),
            (byte)Enums.TokenType.PhoneRegistration => new Tuple<string, DateTime>(profile.PhoneNumberToken!, profile.PhoneNumberTokenTimestamp!.Value),
            _ => default
        };
        if (tokenData is null) return new JsonResult(new StatusCodeResult((int)HttpStatusCode.InternalServerError));

        var (token, tokenTimestamp) = tokenData;
        if (!Equals(token, currentToken)) return new JsonResult(new StatusCodeResult((int)HttpStatusCode.Conflict));
        
        DateTime? tokenElapsingTime = tokenType switch {
            (byte)Enums.TokenType.EmailRegistration => tokenTimestamp.Compute(_emailTokenValidityDuration, _emailTokenValidityDurationUnit),
            (byte)Enums.TokenType.PhoneRegistration => tokenTimestamp.Compute(_phoneTokenValidityDuration, _phoneTokenValidityDurationUnit),
            _ => null
        };
        if (!tokenElapsingTime.HasValue) return new JsonResult(new StatusCodeResult((int)HttpStatusCode.Forbidden));
        if (tokenElapsingTime > DateTime.UtcNow) return new JsonResult(new StatusCodeResult((int)HttpStatusCode.RequestTimeout));

        token = tokenType switch {
            (byte)Enums.TokenType.EmailRegistration => StringHelpers.GenerateRandomString(GetTokenLengthsForNewAccount().Item1, true),
            (byte)Enums.TokenType.PhoneRegistration => StringHelpers.GenerateRandomString(NumberHelpers.GetRandomNumberInRangeInclusive(_phoneTokenMinLength, _phoneTokenMaxLength), true),
            _ => default
        };

        if (!token.IsString()) return new JsonResult(new StatusCodeResult((int)HttpStatusCode.Forbidden));

        var newTokenTimestamp = DateTime.UtcNow;
        await _contextService.StartTransaction();
        
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
            _ => default
        };

        var tokenUpdateResult = updateTokenExpression?.Invoke().Result;
        if (!tokenUpdateResult.HasValue || !tokenUpdateResult.Value) {
            await _contextService.RevertTransaction();
            return new JsonResult(new StatusCodeResult((int)HttpStatusCode.InternalServerError));
        }

        var tokenSendingExpression = tokenType switch {
            (byte)Enums.TokenType.EmailRegistration => (Func<Task<bool?>>)(async () => {
                var newTokenExpiry = newTokenTimestamp.Compute(_emailTokenValidityDuration, _emailTokenValidityDurationUnit).Format(Enums.DateFormat.DDMMYYYYS, Enums.TimeFormat.HHMMTTC);
                
                var mailBinding = new MailBinding {
                    ToReceivers = new List<Recipient> { new() { EmailAddress = account.EmailAddress! } },
                    Title = $"{Constants.ProjectName}: Activate your account",
                    Priority = MailPriority.High,
                    TemplateName = Enums.EmailTemplate.AccountActivationEmail,
                    Placeholders = new Dictionary<string, string> {
                        { "EMAIL_ADDRESS", account.EmailAddress! },
                        { "REGISTRATION_TOKEN", token! },
                        { "VALID_UNTIL_DATETIME", newTokenExpiry }
                    }
                };

                return await _mailService.SendSingleEmail(mailBinding);
            }),
            (byte)Enums.TokenType.PhoneRegistration => async () => {
                var smsContent = _accountActivationSmsContent
                                 .Replace("ACCOUNT_ACTIVATION_TOKEN", token)
                                 .Replace("VALIDITY_DURATION", $"{_phoneTokenValidityDuration} {_phoneTokenValidityDurationUnit}");

                var phoneNumber = JsonConvert.DeserializeObject<RegionalizedPhoneNumber>(profile.PhoneNumber!);
                if (phoneNumber is null) return null;
                
                var smsBinding = new SingleSmsBinding {
                    SmsContent = smsContent,
                    Receivers = new List<string> { phoneNumber.ToString() }
                };

                var smsResult = await _clickatellSmsHttpService.SendSingleSms(smsBinding);
                if (smsResult is null || smsResult.Any()) return null;
                return true;
            },
            _ => null
        };

        var tokenSendingResult = tokenSendingExpression?.Invoke().Result;
        if (!tokenSendingResult.HasValue || !tokenSendingResult.Value) {
            await _contextService.RevertTransaction();
            return new JsonResult(new StatusCodeResult((int)HttpStatusCode.InternalServerError));
        }

        await _contextService.ConfirmTransaction();
        return new JsonResult(new ClientResponse { Result = Enums.ApiResult.Success });
    }

    private Tuple<int, int> GetTokenLengthsForNewAccount() {
        var verificationTokenLength = NumberHelpers.GetRandomNumberInRangeInclusive(_emailTokenMinLength, _emailTokenMaxLength);
        var saltLength = NumberHelpers.GetRandomNumberInRangeInclusive(_saltMinLength, _saltMaxLength);
        return new Tuple<int, int>(verificationTokenLength, saltLength);
    }
}
