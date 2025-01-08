using System.Net;
using AssistantLibrary;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Interfaces.IServiceFactory;
using AssistantLibrary.Services;
using Halogen.Attributes;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings;
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
using Preference = Halogen.Bindings.ServiceBindings.Preference;

namespace Halogen.Controllers;

[ApiController]
[Route("challenges")]
[AutoValidateAntiforgeryToken]
[ServiceFilter(typeof(AuthenticatedAuthorize))]
[ServiceFilter(typeof(TwoFactorAuthorize))]
public sealed class ChallengeController: AppController {
    
    private readonly IContextService _contextService;
    private readonly ICacheService _cacheService;
    private readonly ISessionService _sessionService;
    private readonly IChallengeService _challengeService;
    private readonly IMailService _mailService;
    private readonly ISmsService _smsService;

    public ChallengeController(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration,
        IHaloConfigProvider haloConfigProvider,
        IHaloServiceFactory haloServiceFactory,
        IAssistantServiceFactory assistantServiceFactory
    ): base(ecosystem, logger, configuration, haloConfigProvider.GetHalogenConfigs()) {
        _contextService = haloServiceFactory.GetService<ContextService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<ChallengeController>(nameof(ContextService));
        _sessionService = haloServiceFactory.GetService<SessionService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<ChallengeController>(nameof(SessionService));
        _challengeService = haloServiceFactory.GetService<ChallengeService>(Enums.ServiceType.DbService) ?? throw new HaloArgumentNullException<ChallengeController>(nameof(ChallengeService));
        
        var cacheServiceFactory = haloServiceFactory.GetService<CacheServiceFactory>(Enums.ServiceType.AppService) ?? throw new HaloArgumentNullException<ChallengeController>(nameof(CacheServiceFactory));
        _cacheService = cacheServiceFactory.GetActiveCacheService();
        
        _mailService = assistantServiceFactory.GetService<MailService>() ?? throw new HaloArgumentNullException<ChallengeController>(nameof(MailService));
        _smsService = assistantServiceFactory.GetService<SmsServiceFactory>()?.GetActiveSmsService() ?? throw new HaloArgumentNullException<AuthenticationController>(nameof(SmsServiceFactory));
    }

    /// <summary>
    /// To get all the challenge questions.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     GET /challenges/questions
    ///     Headers
    ///         AccountId: string
    ///         AccessToken: string
    /// </code>
    /// -->
    /// </remarks>
    /// <response code="200">
    /// Successful request with data as follows:
    /// <code>
    /// ChallengeVM {
    ///     id: string,
    ///     question: string,
    /// }
    /// </code>
    /// </response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [HttpGet("questions")]
    public async Task<IActionResult> GetQuestions() {
        _logger.Log(new LoggerBinding<ChallengeController> { Location = nameof(GetQuestions) });

        var questions = await _cacheService.GetCacheEntry<ChallengeVM[]>($"{nameof(ChallengeController)}.{nameof(GetQuestions)}")
                        ?? await _challengeService.GetChallengeQuestions();

        if (questions is null) return new ErrorResponse();
        
        await _cacheService.InsertCacheEntry(new MemoryCacheEntry {
            Key = $"{nameof(ChallengeController)}.{nameof(GetQuestions)}",
            Value = questions,
            Priority = CacheItemPriority.Normal,
        });
            
        return new SuccessResponse(questions);
    }

    /// <summary>
    /// To get all user's responses to challenge questions.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     GET /challenges/responses
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
    /// ChallengeResponseVM {
    ///     id: string,
    ///     challenge: ChallengeVM,
    ///     response: string,
    ///     updatedOn: string,
    /// }
    ///
    /// ChallengeVM {
    ///     id: string,
    ///     question: string,
    /// }
    /// </code>
    /// </response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [HttpGet("responses")]
    public async Task<IActionResult> GetResponses([FromHeader] string accountId) {
        _logger.Log(new LoggerBinding<ChallengeController> { Location = nameof(GetResponses) });

        var responses = await _challengeService.GetAllChallengeResponses(accountId);
        return responses is null ? new ErrorResponse() : new SuccessResponse(responses);
    }

    /// <summary>
    /// To update user's responses to their current challenges. An email will be sent to user to notify that their responses have changed.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     PATCH /challenges/update-response
    ///     Headers
    ///         AccountId: string
    ///         AccessToken: string
    ///     Body
    ///         {
    ///             id: string,
    ///             response: string,
    ///         }
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="accountId">Mapped from header.</param>
    /// <param name="response">ResponseData - Mapped from body.</param>
    /// <response code="200">Successful request.</response>
    /// <response code="400">BadRequest - The response is invalid.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPatch("update-responses")]
    public async Task<IActionResult> UpdateResponses([FromHeader] string accountId, [FromBody] ResponseData response) {
        _logger.Log(new LoggerBinding<ChallengeController> { Location = nameof(UpdateResponses) });

        var errors = response.VerifyResponse();
        if (errors.Count != 0) return new ErrorResponse(HttpStatusCode.BadRequest, errors);
        
        var dbResponse = await _challengeService.GetChallengeResponse(accountId, response.Id);
        if (dbResponse is null) return new ErrorResponse();

        var preference = _sessionService.Get<Preference>($"{Enums.SessionKey.Preference.GetValue()}{accountId}");
        if (preference is null) return new ErrorResponse();
        
        dbResponse.Response = response.Response;

        await _contextService.StartTransaction();
        var responseUpdated = await _challengeService.UpdateChallengeResponse(dbResponse);
        if (responseUpdated.HasValue && responseUpdated.Value) return await NotifySecurityQuestionsChanged();
        
        await _contextService.RevertTransaction();
        return new ErrorResponse();
    }

    /// <summary>
    /// To save the responses to challenge questions. An email will be sent to user to notify the changes.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     POST /challenges/add-responses
    ///     Headers
    ///         AccountId: string
    ///         AccessToken: string
    ///     Body
    ///         {
    ///             challengeId: string,
    ///             response: string,
    ///         }
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="accountId">Mapped from header.</param>
    /// <param name="responses">ChallengeResponseData[] - Mapped from body.</param>
    /// <response code="200">Successful request.</response>
    /// <response code="400">BadRequest - The response is invalid.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpPost("add-responses")]
    public async Task<IActionResult> AddResponses([FromHeader] string accountId, [FromBody] ChallengeResponseData[] responses) {
        _logger.Log(new LoggerBinding<ChallengeController> { Location = nameof(AddResponses) });
        
        var errors = responses.SelectMany(response => response.VerifyResponse()).ToArray();
        if (errors.Length != 0) return new ErrorResponse(HttpStatusCode.BadRequest, errors);

        var challengeResponses = responses.Select(response => new ChallengeResponse {
            AccountId = accountId,
            ChallengeId = response.ChallengeId,
            Response = response.Response,
        }).ToArray();

        var recordsAdded = await _challengeService.AddChallengeResponsesMulti(challengeResponses);
        if (recordsAdded.HasValue && recordsAdded.Value) return await NotifySecurityQuestionsChanged();
        
        await _contextService.RevertTransaction();
        return new SuccessResponse();
    }

    /// <summary>
    /// To remove the response to user's challenge questions.
    /// </summary>
    /// <remarks>
    /// Request signature:
    /// <!--
    /// <code>
    ///     DELETE /challenges/remove-response
    ///     Headers
    ///         AccountId: string
    ///         AccessToken: string
    /// </code>
    /// -->
    /// </remarks>
    /// <param name="accountId">Mapped from header.</param>
    /// <param name="responseId">Mapped from route.</param>
    /// <response code="200">Successful request.</response>
    /// <response code="500">Internal Server Error - Something went wrong with Halogen services.</response>
    [ServiceFilter(typeof(RecaptchaAuthorize))]
    [HttpDelete("remove-response/{responseId}")]
    public async Task<IActionResult> RemoveResponses([FromHeader] string accountId, [FromRoute] string responseId) {
        _logger.Log(new LoggerBinding<ChallengeController> { Location = nameof(RemoveResponses) });
        
        var response = await _challengeService.GetChallengeResponse(accountId, responseId);
        if (response is null) return new ErrorResponse();

        var responseDeleted = await _challengeService.DeleteChallengeResponse(response);
        if (responseDeleted.HasValue && responseDeleted.Value) return await NotifySecurityQuestionsChanged();
        
        await _contextService.RevertTransaction();
        return new SuccessResponse();
    }

    private async Task<IActionResult> NotifySecurityQuestionsChanged() {
        _logger.Log(new LoggerBinding<ChallengeController> { IsPrivate = true, Location = nameof(NotifySecurityQuestionsChanged) });
        
        var notificationSent = await SendNotification(new NotificationContent {
            Title = $"{Constants.ProjectName}: Security Questions Changed",
            MailTemplateName = Enums.EmailTemplate.SecurityQuestionsChangedNotification,
            SmsContent = _haloConfigs.SecurityQuestionsChangedSms,
        }, _mailService, _smsService);
        
        if (!notificationSent.HasValue || !notificationSent.Value) {
            await _contextService.RevertTransaction();
            return new ErrorResponse();
        }

        await _contextService.ConfirmTransaction();
        return new SuccessResponse();
    }
}