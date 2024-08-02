using System.Net;
using AssistantLibrary;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Services;
using Halogen.Attributes;
using Halogen.Bindings;
using Halogen.Bindings.ServiceBindings;
using Halogen.Bindings.ViewModels;
using HaloUnitTest.Mocks;
using HaloUnitTest.Mocks.Assistants;
using HaloUnitTest.Mocks.HaloApi.Auxiliaries;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace HaloUnitTest.HalogenApiTests.Attributes;

[TestFixture]
public sealed class RecaptchaAuthorizeTest {
    
    private ILoggerService _loggerMock;
    private IConfiguration _configMock;
    private IAssistantServiceFactory _assistantSvFactoryMock = null!;
    private AuthorizationFilterContext _authorizationFilterCtx = null!;
    private static readonly string ConfigKey = $"{nameof(HalogenOptions)}{Constants.Colon}{MockBase.EcosystemMock.GetEnvironment()}{Constants.Colon}{nameof(HalogenOptions.Local.ServiceSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.ServiceSettings.RecaptchaEnabled)}";

    [OneTimeSetUp]
    public void SetUp() {
        _loggerMock = MockBase.Simulate<ILoggerService>().Object;
        _configMock = ConfigurationMock.Instance(new KeyValuePair<string, string>(ConfigKey, "True"));
        
        MockAssistantServiceFactory(out _assistantSvFactoryMock, new RecaptchaResponse {
            IsHuman = true,
            HostName = "NUnit",
        });
        
        MockAuthorizationFilterContext(out _authorizationFilterCtx, new Dictionary<string, StringValues> {
            { nameof(HttpHeaderKeys.RecaptchaToken), "3fc9b689459d738f8c88a3a48aa9e33542016b7a4052e001aaa536fca74813cb" },
        });
    }

    [TearDown]
    public void TearDown() {
        _configMock = ConfigurationMock.Instance(new KeyValuePair<string, string>(ConfigKey, "True"));
        
        MockAssistantServiceFactory(out _assistantSvFactoryMock, new RecaptchaResponse {
            IsHuman = true,
            HostName = "NUnit",
        });
        
        MockAuthorizationFilterContext(out _authorizationFilterCtx, new Dictionary<string, StringValues> {
            { nameof(HttpHeaderKeys.RecaptchaToken), "3fc9b689459d738f8c88a3a48aa9e33542016b7a4052e001aaa536fca74813cb" },
        });
    }

    [Test]
    public void Test_OnAuthorization_RecaptchaDisabled() {
        _configMock = ConfigurationMock.Instance(new KeyValuePair<string, string>(ConfigKey, "False"));
        var recaptchaAuthorize = new RecaptchaAuthorize(MockBase.EcosystemMock, _loggerMock, _configMock, _assistantSvFactoryMock);
        recaptchaAuthorize.OnAuthorization(_authorizationFilterCtx);
        
        var result = _authorizationFilterCtx.Result as ErrorResponse;
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Test_OnAuthorization_NoRecaptchaToken() {
        MockAuthorizationFilterContext(out _authorizationFilterCtx, new Dictionary<string, StringValues> {
            { nameof(HttpHeaderKeys.RecaptchaToken), "" },
        });
        
        var recaptchaAuthorize = new RecaptchaAuthorize(MockBase.EcosystemMock, _loggerMock, _configMock, _assistantSvFactoryMock);
        recaptchaAuthorize.OnAuthorization(_authorizationFilterCtx);
        
        var result = _authorizationFilterCtx.Result as ErrorResponse;
        Assert.That(result, Is.Not.Null);
        
        var expect = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.NoRecaptchaToken.GetValue()}");
        Assert.Multiple(() => {
            Assert.That(result.StatusCode, Is.EqualTo(expect.StatusCode));
            Assert.That(result.Content, Is.EqualTo(expect.Content));
        });
    }

    [Test]
    public void Test_OnAuthorization_RecaptchaFailure() {
        MockAssistantServiceFactory(out _assistantSvFactoryMock, new RecaptchaResponse {
            IsHuman = false,
            HostName = "NUnit",
        });
        
        var recaptchaAuthorize = new RecaptchaAuthorize(MockBase.EcosystemMock, _loggerMock, _configMock, _assistantSvFactoryMock);
        recaptchaAuthorize.OnAuthorization(_authorizationFilterCtx);
        
        var result = _authorizationFilterCtx.Result as ErrorResponse;
        Assert.That(result, Is.Not.Null);
        
        var expect = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.RecaptchaNotAHuman.GetValue()}");
        Assert.Multiple(() => {
            Assert.That(result.StatusCode, Is.EqualTo(expect.StatusCode));
            Assert.That(result.Content, Is.EqualTo(expect.Content));
        });
    }
    
    [Test]
    public void Test_OnAuthorization_RecaptchaSuccess() {
        var recaptchaAuthorize = new RecaptchaAuthorize(MockBase.EcosystemMock, _loggerMock, _configMock, _assistantSvFactoryMock);
        recaptchaAuthorize.OnAuthorization(_authorizationFilterCtx);
        
        var result = _authorizationFilterCtx.Result as ErrorResponse;
        Assert.That(result, Is.Null);
    }

    private static void MockAssistantServiceFactory(out IAssistantServiceFactory assistantSvFactory, RecaptchaResponse recaptchaResponse) {
        var assistantSvMock = AssistantServiceMock.Instance<RecaptchaResponse>([new KeyValuePair<string, RecaptchaResponse>(nameof(IAssistantService.IsHumanActivity), recaptchaResponse)]);
        assistantSvFactory = AssistantServiceFactoryMock.Instance<AssistantService>([assistantSvMock]);
    }

    private static void MockAuthorizationFilterContext(out AuthorizationFilterContext authFilterCtx, Dictionary<string, StringValues> requestHeaders) =>
        authFilterCtx = AuthorizationFilterContextMock.Actual([
            new KeyValuePair<string, Dictionary<string, StringValues>>(nameof(HttpContext.Request.Headers), requestHeaders),
        ]);
}
