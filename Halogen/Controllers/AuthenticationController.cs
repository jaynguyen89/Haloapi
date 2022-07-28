using System.Net.Mail;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
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
using Microsoft.Extensions.Options;

namespace Halogen.Controllers; 

[ApiController]
[Route("authentication")]
internal sealed class AuthenticationController: AppController {

    private readonly IContextService _contextService;
    private readonly IAuthenticationService _authenticationService;
    private readonly ICryptoService _cryptoService;
    private readonly IMailService _mailService;
    private readonly IAccountService _accountService;
    private readonly IProfileService _profileService;
    private readonly IRoleService _roleService;
    private readonly IPreferenceService _preferenceService;

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

    internal AuthenticationController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IOptions<HalogenOptions> options,
        IContextService contextService,
        IAuthenticationService authenticationService,
        ICryptoService cryptoService,
        IMailService mailService,
        IAccountService accountService,
        IProfileService profileService,
        IRoleService roleService,
        IPreferenceService preferenceService
    ) : base(ecosystem, logger, options) {
        _contextService = contextService;
        _authenticationService = authenticationService;
        _cryptoService = cryptoService;
        _mailService = mailService;
        _accountService = accountService;
        _profileService = profileService;
        _roleService = roleService;
        _preferenceService = preferenceService;
        ParseSecuritySettings(
            out _saltMinLength, out _saltMaxLength, out _tfaKeyMinLength, out _tfaKeyMaxLength,
            out _emailTokenMinLength, out _emailTokenMaxLength, out _emailTokenValidityDuration, out _emailTokenValidityDurationUnit,
            out _otpMinLength, out _otpMaxLength, out _otpValidityDuration, out _otpValidityDurationUnit,
            out _recoveryTokenMinLength, out _recoveryTokenMaxLength, out _recoveryTokenValidityDuration, out _recoveryTokenValidityDurationUnit,
            out _phoneTokenMinLength, out _phoneTokenMaxLength, out _phoneTokenValidityDuration, out _phoneTokenValidityDurationUnit,
            out _loginFailedThreshold, out _lockOutThreshold, out _lockOutDuration, out _lockOutDurationUnit
        );
    }

    private void ParseSecuritySettings(
        out int saltMinLength, out int saltMaxLength, out int tfaKeyMinLength, out int tfaKeyMaxLength,
        out int emailTokenMinLength, out int emailTokenMaxLength, out int emailTokenValidityDuration, out Enums.TimeUnit emailTokenValidityDurationUnit,
        out int otpMinLength, out int otpMaxLength, out int otpValidityDuration, out Enums.TimeUnit otpValidityDurationUnit,
        out int recoveryTokenMinLength, out int recoveryTokenMaxLength, out int recoveryTokenValidityDuration, out Enums.TimeUnit recoveryTokenValidityDurationUnit,
        out int phoneTokenMinLength, out int phoneTokenMaxLength, out int phoneTokenValidityDuration, out Enums.TimeUnit phoneTokenValidityDurationUnit,
        out int loginFailedThreshold, out int lockOutThreshold, out int lockOutDuration, out Enums.TimeUnit lockOutDurationUnit
    ) {
        (
            saltMinLength, saltMaxLength, tfaKeyMinLength, tfaKeyMaxLength,
            emailTokenMinLength, emailTokenMaxLength, emailTokenValidityDuration, emailTokenValidityDurationUnit
        ) = _environment switch {
            Constants.Development => (
                int.Parse(_options.Dev.SecuritySettings.SaltMinLength),
                int.Parse(_options.Dev.SecuritySettings.SaltMaxLength),
                int.Parse(_options.Dev.SecuritySettings.TfaKeyMinLength),
                int.Parse(_options.Dev.SecuritySettings.TfaKeyMaxLength),
                int.Parse(_options.Dev.SecuritySettings.EmailTokenMinLength),
                int.Parse(_options.Dev.SecuritySettings.EmailTokenMaxLength),
                int.Parse(_options.Dev.SecuritySettings.EmailTokenValidityDuration),
                _options.Dev.SecuritySettings.EmailTokenValidityDurationUnit.ToEnum(Enums.TimeUnit.HOUR)
            ),
            Constants.Staging => (
                int.Parse(_options.Stg.SecuritySettings.SaltMinLength),
                int.Parse(_options.Stg.SecuritySettings.SaltMaxLength),
                int.Parse(_options.Stg.SecuritySettings.TfaKeyMinLength),
                int.Parse(_options.Stg.SecuritySettings.TfaKeyMaxLength),
                int.Parse(_options.Stg.SecuritySettings.EmailTokenMinLength),
                int.Parse(_options.Stg.SecuritySettings.EmailTokenMaxLength),
                int.Parse(_options.Stg.SecuritySettings.EmailTokenValidityDuration),
                _options.Stg.SecuritySettings.EmailTokenValidityDurationUnit.ToEnum(Enums.TimeUnit.HOUR)
            ),
            Constants.Production => (
                int.Parse(_options.Prod.SecuritySettings.SaltMinLength),
                int.Parse(_options.Prod.SecuritySettings.SaltMaxLength),
                int.Parse(_options.Prod.SecuritySettings.TfaKeyMinLength),
                int.Parse(_options.Prod.SecuritySettings.TfaKeyMaxLength),
                int.Parse(_options.Prod.SecuritySettings.EmailTokenMinLength),
                int.Parse(_options.Prod.SecuritySettings.EmailTokenMaxLength),
                int.Parse(_options.Prod.SecuritySettings.EmailTokenValidityDuration),
                _options.Prod.SecuritySettings.EmailTokenValidityDurationUnit.ToEnum(Enums.TimeUnit.HOUR)
            ),
            _ => (
                int.Parse(_options.Loc.SecuritySettings.SaltMinLength),
                int.Parse(_options.Loc.SecuritySettings.SaltMaxLength),
                int.Parse(_options.Loc.SecuritySettings.TfaKeyMinLength),
                int.Parse(_options.Loc.SecuritySettings.TfaKeyMaxLength),
                int.Parse(_options.Loc.SecuritySettings.EmailTokenMinLength),
                int.Parse(_options.Loc.SecuritySettings.EmailTokenMaxLength),
                int.Parse(_options.Loc.SecuritySettings.EmailTokenValidityDuration),
                _options.Loc.SecuritySettings.EmailTokenValidityDurationUnit.ToEnum(Enums.TimeUnit.HOUR)
            )
        };
        
        (
            otpMinLength, otpMaxLength, otpValidityDuration, otpValidityDurationUnit,
            recoveryTokenMinLength, recoveryTokenMaxLength, recoveryTokenValidityDuration, recoveryTokenValidityDurationUnit
        ) = _environment switch {
            Constants.Development => (
                int.Parse(_options.Dev.SecuritySettings.OtpMinLength),
                int.Parse(_options.Dev.SecuritySettings.OtpMaxLength),
                int.Parse(_options.Dev.SecuritySettings.OtpValidityDuration),
                _options.Dev.SecuritySettings.OtpValidityDurationUnit.ToEnum(Enums.TimeUnit.HOUR),
                int.Parse(_options.Dev.SecuritySettings.RecoveryTokenMinLength),
                int.Parse(_options.Dev.SecuritySettings.RecoveryTokenMaxLength),
                int.Parse(_options.Dev.SecuritySettings.RecoveryTokenValidityDuration),
                _options.Dev.SecuritySettings.RecoveryTokenValidityDurationUnit.ToEnum(Enums.TimeUnit.HOUR)
            ),
            Constants.Staging => (
                int.Parse(_options.Stg.SecuritySettings.OtpMinLength),
                int.Parse(_options.Stg.SecuritySettings.OtpMaxLength),
                int.Parse(_options.Stg.SecuritySettings.OtpValidityDuration),
                _options.Stg.SecuritySettings.OtpValidityDurationUnit.ToEnum(Enums.TimeUnit.HOUR),
                int.Parse(_options.Stg.SecuritySettings.RecoveryTokenMinLength),
                int.Parse(_options.Stg.SecuritySettings.RecoveryTokenMaxLength),
                int.Parse(_options.Stg.SecuritySettings.RecoveryTokenValidityDuration),
                _options.Stg.SecuritySettings.RecoveryTokenValidityDurationUnit.ToEnum(Enums.TimeUnit.HOUR)
            ),
            Constants.Production => (
                int.Parse(_options.Prod.SecuritySettings.OtpMinLength),
                int.Parse(_options.Prod.SecuritySettings.OtpMaxLength),
                int.Parse(_options.Prod.SecuritySettings.OtpValidityDuration),
                _options.Prod.SecuritySettings.OtpValidityDurationUnit.ToEnum(Enums.TimeUnit.HOUR),
                int.Parse(_options.Prod.SecuritySettings.RecoveryTokenMinLength),
                int.Parse(_options.Prod.SecuritySettings.RecoveryTokenMaxLength),
                int.Parse(_options.Prod.SecuritySettings.RecoveryTokenValidityDuration),
                _options.Prod.SecuritySettings.RecoveryTokenValidityDurationUnit.ToEnum(Enums.TimeUnit.HOUR)
            ),
            _ => (
                int.Parse(_options.Loc.SecuritySettings.OtpMinLength),
                int.Parse(_options.Loc.SecuritySettings.OtpMaxLength),
                int.Parse(_options.Loc.SecuritySettings.OtpValidityDuration),
                _options.Loc.SecuritySettings.OtpValidityDurationUnit.ToEnum(Enums.TimeUnit.HOUR),
                int.Parse(_options.Loc.SecuritySettings.RecoveryTokenMinLength),
                int.Parse(_options.Loc.SecuritySettings.RecoveryTokenMaxLength),
                int.Parse(_options.Loc.SecuritySettings.RecoveryTokenValidityDuration),
                _options.Loc.SecuritySettings.RecoveryTokenValidityDurationUnit.ToEnum(Enums.TimeUnit.HOUR)
            )
        };
        
        (
            phoneTokenMinLength, phoneTokenMaxLength, phoneTokenValidityDuration, phoneTokenValidityDurationUnit,
            loginFailedThreshold, lockOutThreshold, lockOutDuration, lockOutDurationUnit
        ) = _environment switch {
            Constants.Development => (
                int.Parse(_options.Dev.SecuritySettings.PhoneTokenMinLength),
                int.Parse(_options.Dev.SecuritySettings.PhoneTokenMaxLength),
                int.Parse(_options.Dev.SecuritySettings.PhoneTokenValidityDuration),
                _options.Dev.SecuritySettings.PhoneTokenValidityDurationUnit.ToEnum(Enums.TimeUnit.HOUR),
                int.Parse(_options.Dev.SecuritySettings.LoginFailedThreshold),
                int.Parse(_options.Dev.SecuritySettings.LockOutThreshold),
                int.Parse(_options.Dev.SecuritySettings.LockOutDuration),
                _options.Dev.SecuritySettings.LockOutDurationUnit.ToEnum(Enums.TimeUnit.HOUR)
            ),
            Constants.Staging => (
                int.Parse(_options.Stg.SecuritySettings.PhoneTokenMinLength),
                int.Parse(_options.Stg.SecuritySettings.PhoneTokenMaxLength),
                int.Parse(_options.Stg.SecuritySettings.PhoneTokenValidityDuration),
                _options.Stg.SecuritySettings.PhoneTokenValidityDurationUnit.ToEnum(Enums.TimeUnit.HOUR),
                int.Parse(_options.Stg.SecuritySettings.LoginFailedThreshold),
                int.Parse(_options.Stg.SecuritySettings.LockOutThreshold),
                int.Parse(_options.Stg.SecuritySettings.LockOutDuration),
                _options.Stg.SecuritySettings.LockOutDurationUnit.ToEnum(Enums.TimeUnit.HOUR)
            ),
            Constants.Production => (
                int.Parse(_options.Prod.SecuritySettings.PhoneTokenMinLength),
                int.Parse(_options.Prod.SecuritySettings.PhoneTokenMaxLength),
                int.Parse(_options.Prod.SecuritySettings.PhoneTokenValidityDuration),
                _options.Prod.SecuritySettings.PhoneTokenValidityDurationUnit.ToEnum(Enums.TimeUnit.HOUR),
                int.Parse(_options.Prod.SecuritySettings.LoginFailedThreshold),
                int.Parse(_options.Prod.SecuritySettings.LockOutThreshold),
                int.Parse(_options.Prod.SecuritySettings.LockOutDuration),
                _options.Prod.SecuritySettings.LockOutDurationUnit.ToEnum(Enums.TimeUnit.HOUR)
            ),
            _ => (
                int.Parse(_options.Loc.SecuritySettings.PhoneTokenMinLength),
                int.Parse(_options.Loc.SecuritySettings.PhoneTokenMaxLength),
                int.Parse(_options.Loc.SecuritySettings.PhoneTokenValidityDuration),
                _options.Loc.SecuritySettings.PhoneTokenValidityDurationUnit.ToEnum(Enums.TimeUnit.HOUR),
                int.Parse(_options.Loc.SecuritySettings.LoginFailedThreshold),
                int.Parse(_options.Loc.SecuritySettings.LockOutThreshold),
                int.Parse(_options.Loc.SecuritySettings.LockOutDuration),
                _options.Loc.SecuritySettings.LockOutDurationUnit.ToEnum(Enums.TimeUnit.HOUR)
            )
        };
    }

    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPost("register-by-email-address")]
    public async Task<JsonResult> RegisterAccountByEmailAddress(RegistrationData registrationData) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(RegisterAccountByEmailAddress) });

        var errors = await registrationData.VerifyRegistrationData();
        if (errors.Any()) return new JsonResult(new ClientResponse { Result = Enums.ApiResult.FAILED, Data = errors });

        var isEmailAvailable = await _accountService.IsEmailAvailableForNewAccount(registrationData.EmailAddress!);
        if (!isEmailAvailable.HasValue) return new JsonResult(new ClientResponse { Result = Enums.ApiResult.FAILED });
        if (!isEmailAvailable.Value) return new JsonResult(new ClientResponse { Result = Enums.ApiResult.FAILED, Data = new { isEmailAvailable }});

        var (verificationTokenLength, saltLength) = GetTokenLengthsForNewAccount();
        var (hashedPassword, salt) = _cryptoService.GenerateHashAndSalt(registrationData.Password, saltLength);

        var newAccount = Account.CreateNewAccount(_useLongerId, registrationData.EmailAddress!, salt, hashedPassword, verificationTokenLength);
        await _contextService.StartTransaction();

        var accountId = await _authenticationService.InsertNewAccount(newAccount);
        if (accountId is null) {
            await _contextService.RevertTransaction();
            return new JsonResult(new ClientResponse { Result = Enums.ApiResult.FAILED });
        }
        
        var profileId = await _profileService.InsertNewProfile(new Profile {
            Id = StringHelpers.NewGuid(_useLongerId),
            AccountId = accountId
        });
        if (profileId is null) {
            await _contextService.RevertTransaction();
            return new JsonResult(new ClientResponse { Result = Enums.ApiResult.FAILED });
        }

        var role = await _roleService.GetRoleByName(Enums.Role.CUSTOMER.GetEnumValue<string>()!);
        if (role is null) {
            await _contextService.RevertTransaction();
            return new JsonResult(new ClientResponse { Result = Enums.ApiResult.FAILED });
        }

        var accountRoleId = await _roleService.InsertNewAccountRole(new AccountRole {
            Id = StringHelpers.NewGuid(_useLongerId),
            AccountId = accountId,
            RoleId = role.Id,
            IsEffective = true
        });
        if (accountRoleId is null) {
            await _contextService.RevertTransaction();
            return new JsonResult(new ClientResponse { Result = Enums.ApiResult.FAILED });
        }

        var defaultPreference = Preference.CreatePreferenceForNewAccount(_useLongerId, accountId);
        var preferenceId = await _preferenceService.InsertNewPreference(defaultPreference);
        if (preferenceId is null) {
            await _contextService.RevertTransaction();
            return new JsonResult(new ClientResponse { Result = Enums.ApiResult.FAILED });
        }

        var accountActivationEmailExpiry = newAccount.EmailAddressTokenTimestamp!.Value
            .Compute(_emailTokenValidityDuration, _emailTokenValidityDurationUnit)
            .Format(Enums.DateFormat.DDMMYYYYS, Enums.TimeFormat.HHMMTTC);
        
        var isAccountActivationEmailSent = await SendAccountActivationEmail(newAccount.EmailAddress, newAccount.EmailAddressToken!, accountActivationEmailExpiry!);
        if (!isAccountActivationEmailSent) {
            await _contextService.RevertTransaction();
            return new JsonResult(new ClientResponse { Result = Enums.ApiResult.FAILED });
        }

        await _contextService.ConfirmTransaction();
        return new JsonResult(new ClientResponse { Result = Enums.ApiResult.SUCCESS });
    }

    private async Task<bool> SendAccountActivationEmail(string emailAddress, string verificationToken, string emailExpiryTimestamp) {
        _logger.Log(new LoggerBinding<AuthenticationController> { Location = nameof(SendAccountActivationEmail), IsPrivate = true });
        var mailBinding = new MailBinding {
            ToReceivers = new List<Recipient> { new() { EmailAddress = emailAddress } },
            Title = $"{Constants.ProjectName}: Activate your account",
            Priority = MailPriority.High,
            TemplateName = Enums.EmailTemplate.ACCOUNT_ACTIVATION_EMAIL,
            Placeholders = new Dictionary<string, string> {
                { "EMAIL_ADDRESS", emailAddress },
                { "REGISTRATION_TOKEN", verificationToken },
                { "VALID_UNTIL_DATETIME", emailExpiryTimestamp }
            }
        };

        return await _mailService.SendSingleEmail(mailBinding);
    }

    private Tuple<int, int> GetTokenLengthsForNewAccount() {
        var verificationTokenLength = NumberHelpers.GetRandomNumberInRangeInclusive(_emailTokenMinLength, _emailTokenMaxLength);
        var saltLength = NumberHelpers.GetRandomNumberInRangeInclusive(_saltMinLength, _saltMaxLength);
        return new Tuple<int, int>(verificationTokenLength, saltLength);
    }
}
