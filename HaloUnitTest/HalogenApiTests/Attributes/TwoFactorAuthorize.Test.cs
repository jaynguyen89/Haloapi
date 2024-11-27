using System.Net;
using AssistantLibrary;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using Halogen.Attributes;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings;
using Halogen.Bindings.ServiceBindings;
using Halogen.Bindings.ViewModels;
using Halogen.Services.AppServices.Interfaces;
using HaloUnitTest.Mocks;
using HaloUnitTest.Mocks.Assistants;
using HaloUnitTest.Mocks.HaloApi.Auxiliaries;
using HaloUnitTest.Mocks.HaloApi.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace HaloUnitTest.HalogenApiTests.Attributes;

[TestFixture]
public sealed class TwoFactorAuthorizeTest {

    private ILoggerService _loggerMock;
    private IConfiguration _configMock = null!;
    private ISessionService _sessionSvMock = null!;
    private IHaloServiceFactory _haloSvFactoryMock = null!;
    private ITwoFactorService _twoFactorSvMock = null!;
    private IAssistantServiceFactory _assistantSvFactoryMock = null!;
    private AuthorizationFilterContext _authFilterCtx = null!;

    [OneTimeSetUp]
    public void SetUp() {
        _loggerMock = MockBase.Simulate<ILoggerService>().Object;
        MockConfiguration(out _configMock);
        MockSessionService(out _sessionSvMock, "098f6bcd4621d373cade4e832627b4f6");
        MockHaloServiceFactory(out _haloSvFactoryMock, _sessionSvMock);
        MockTwoFactorServiceVerification(out _twoFactorSvMock);
        MockAssistantServiceFactory(out _assistantSvFactoryMock, _twoFactorSvMock);
        MockAuthorizationFilterContext(out _authFilterCtx, new KeyValuePair<string, string>(nameof(HttpHeaderKeys.TwoFactorToken), "4ec503be252d765ea37621a629afdaa6"));
    }

    [TearDown]
    public void TearDown() {
        MockConfiguration(out _configMock);
        MockSessionService(out _sessionSvMock, "098f6bcd4621d373cade4e832627b4f6");
        MockHaloServiceFactory(out _haloSvFactoryMock, _sessionSvMock);
        MockTwoFactorServiceVerification(out _twoFactorSvMock);
        MockAssistantServiceFactory(out _assistantSvFactoryMock, _twoFactorSvMock);
        MockAuthorizationFilterContext(out _authFilterCtx, new KeyValuePair<string, string>(nameof(HttpHeaderKeys.TwoFactorToken), "4ec503be252d765ea37621a629afdaa6"));
        _authFilterCtx.Result = null;
    }

    [Test]
    public void Test_OnAuthorization_TFADisabled() {
        MockConfiguration(out _configMock, false);

        var tfaAuthorize = new TwoFactorAuthorize(MockBase.EcosystemMock, _loggerMock, _configMock, _haloSvFactoryMock, _assistantSvFactoryMock);
        tfaAuthorize.OnAuthorization(_authFilterCtx);

        var result = _authFilterCtx.Result as ErrorResponse;
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public void Test_OnAuthorization_TFANoSecretKey() {
        MockSessionService(out _sessionSvMock, string.Empty);
        
        var tfaAuthorize = new TwoFactorAuthorize(MockBase.EcosystemMock, _loggerMock, _configMock, _haloSvFactoryMock, _assistantSvFactoryMock);
        tfaAuthorize.OnAuthorization(_authFilterCtx);

        var result = _authFilterCtx.Result as ErrorResponse;
        Assert.That(result, Is.Not.Null);

        var expect = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(TwoFactorAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.NoTwoFactorToken.GetValue()}");
        Assert.Multiple(() => {
            Assert.That(result.StatusCode, Is.EqualTo(expect.StatusCode));
            Assert.That(result.Content, Is.EqualTo(expect.Content));
        });
    }
    
    [Test]
    public void Test_OnAuthorization_TFANoToken() {
        MockAuthorizationFilterContext(out _authFilterCtx, new KeyValuePair<string, string>(nameof(HttpHeaderKeys.TwoFactorToken), string.Empty));
        
        var tfaAuthorize = new TwoFactorAuthorize(MockBase.EcosystemMock, _loggerMock, _configMock, _haloSvFactoryMock, _assistantSvFactoryMock);
        tfaAuthorize.OnAuthorization(_authFilterCtx);

        var result = _authFilterCtx.Result as ErrorResponse;
        Assert.That(result, Is.Not.Null);

        var expect = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(TwoFactorAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.NoTwoFactorToken.GetValue()}");
        Assert.Multiple(() => {
            Assert.That(result.StatusCode, Is.EqualTo(expect.StatusCode));
            Assert.That(result.Content, Is.EqualTo(expect.Content));
        });
    }
    
    [Test]
    public void Test_OnAuthorization_TFAInvalid() {
        MockTwoFactorServiceVerification(out _twoFactorSvMock, false);
        MockAssistantServiceFactory(out _assistantSvFactoryMock, _twoFactorSvMock);
        
        var tfaAuthorize = new TwoFactorAuthorize(MockBase.EcosystemMock, _loggerMock, _configMock, _haloSvFactoryMock, _assistantSvFactoryMock);
        tfaAuthorize.OnAuthorization(_authFilterCtx);

        var result = _authFilterCtx.Result as ErrorResponse;
        Assert.That(result, Is.Not.Null);

        var expect = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(TwoFactorAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.NoTwoFactorToken.GetValue()}");
        Assert.Multiple(() => {
            Assert.That(result.StatusCode, Is.EqualTo(expect.StatusCode));
            Assert.That(result.Content, Is.EqualTo(expect.Content));
        });
    }
    
    [Test]
    public void Test_OnAuthorization_TFASuccess() {
        var tfaAuthorize = new TwoFactorAuthorize(MockBase.EcosystemMock, _loggerMock, _configMock, _haloSvFactoryMock, _assistantSvFactoryMock);
        tfaAuthorize.OnAuthorization(_authFilterCtx);
    
        var result = _authFilterCtx.Result as ErrorResponse;
        Assert.That(result, Is.Null);
    }

    private static void MockConfiguration(out IConfiguration configMock, bool tfaEnabled = true) =>
        configMock = ConfigurationMock.Instance(new KeyValuePair<string, string>(
            $"{nameof(HalogenOptions)}{Constants.Colon}{MockBase.EcosystemMock.GetEnvironment()}{Constants.Colon}{nameof(HalogenOptions.Local.ServiceSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.ServiceSettings.TwoFactorEnabled)}",
            tfaEnabled ? "True" : "False"
        ));

    private static void MockSessionService(out ISessionService sessionSvMock, string val) =>
        sessionSvMock = SessionServiceMock.Instance<string>([new KeyValuePair<string, string>(nameof(HttpHeaderKeys.TwoFactorToken), val)]);

    private static void MockHaloServiceFactory<T>(out IHaloServiceFactory haloSvFactory, T service) =>
        haloSvFactory = HaloServiceFactoryMock.Instance<T>([new KeyValuePair<Enums.ServiceType, T>(Enums.ServiceType.AppService, service)]);

    private static void MockTwoFactorServiceVerification(out ITwoFactorService twoFactorSvMock, bool val = true) =>
        twoFactorSvMock = TwoFactorServiceMock.Instance(
            nameof(ITwoFactorService.VerifyTwoFactorAuthenticationPin),
            new VerifyTwoFactorBinding {
                PinCode = string.Empty,
                SecretKey = string.Empty,
            },
            val ? "True" : "False"
        );

    private static void MockAssistantServiceFactory<T>(out IAssistantServiceFactory assistantSvFactoryMock, T service) =>
        assistantSvFactoryMock = AssistantServiceFactoryMock.Instance<T>([service]);

    private static void MockAuthorizationFilterContext(out AuthorizationFilterContext authFilterCtx, KeyValuePair<string, string> keyVal) {
        var (key, val) = keyVal;
        var requestHeaders = new Dictionary<string, StringValues> {{ key, val }};
        authFilterCtx = AuthorizationFilterContextMock.Actual([
            new KeyValuePair<string, Dictionary<string, StringValues>>(nameof(HttpContext.Request.Headers), requestHeaders),
        ]);
    }
}
