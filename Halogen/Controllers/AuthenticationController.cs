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
        var baseOptionKey = _environment switch {
            Constants.Development => $"{nameof(HalogenOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(HalogenOptions.Development.SecuritySettings)}{Constants.Colon}",
            Constants.Staging => $"{nameof(HalogenOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(HalogenOptions.Staging.SecuritySettings)}{Constants.Colon}",
            Constants.Production => $"{nameof(HalogenOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(HalogenOptions.Production.SecuritySettings)}{Constants.Colon}",
            _ => $"{nameof(HalogenOptions)}{Constants.Colon}{_environment}{Constants.Colon}{nameof(HalogenOptions.Local.SecuritySettings)}{Constants.Colon}"
        };

        (
            saltMinLength, saltMaxLength, tfaKeyMinLength, tfaKeyMaxLength,
            emailTokenMinLength, emailTokenMaxLength, emailTokenValidityDuration, emailTokenValidityDurationUnit
        ) = (
            int.Parse(_configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.SaltMinLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.SaltMaxLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.TfaKeyMinLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.TfaKeyMaxLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.EmailTokenMinLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.EmailTokenMaxLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.EmailTokenValidityDuration)}")),
            _configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.EmailTokenValidityDurationUnit)}").ToEnum(Enums.TimeUnit.HOUR)
        );
        
        (
            otpMinLength, otpMaxLength, otpValidityDuration, otpValidityDurationUnit,
            recoveryTokenMinLength, recoveryTokenMaxLength, recoveryTokenValidityDuration, recoveryTokenValidityDurationUnit
        ) = (
            int.Parse(_configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.OtpMinLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.OtpMaxLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.OtpValidityDuration)}")),
            _configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.OtpValidityDurationUnit)}").ToEnum(Enums.TimeUnit.HOUR),
            int.Parse(_configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.RecoveryTokenMinLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.RecoveryTokenMaxLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.RecoveryTokenValidityDuration)}")),
            _configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.RecoveryTokenValidityDurationUnit)}").ToEnum(Enums.TimeUnit.HOUR)
        );
        
        (
            phoneTokenMinLength, phoneTokenMaxLength, phoneTokenValidityDuration, phoneTokenValidityDurationUnit,
            loginFailedThreshold, lockOutThreshold, lockOutDuration, lockOutDurationUnit
        ) = (
            int.Parse(_configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.PhoneTokenMinLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.PhoneTokenMaxLength)}")),
            int.Parse(_configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.PhoneTokenValidityDuration)}")),
            _configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.PhoneTokenValidityDurationUnit)}").ToEnum(Enums.TimeUnit.HOUR),
            int.Parse(_configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.LoginFailedThreshold)}")),
            int.Parse(_configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.LockOutThreshold)}")),
            int.Parse(_configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.LockOutDuration)}")),
            _configuration.GetValue<string>($"{baseOptionKey}{nameof(HalogenOptions.Local.SecuritySettings.LockOutDurationUnit)}").ToEnum(Enums.TimeUnit.HOUR)
        );
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
