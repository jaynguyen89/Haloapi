using System.Net;
using Halogen.Attributes;
using Halogen.Bindings;
using Halogen.Bindings.ServiceBindings;
using Halogen.Bindings.ViewModels;
using HaloUnitTest.Mocks;
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
public sealed class TwoFactorAuthorizeTest {

    private ILoggerService _loggerMock;
    private IConfiguration _configMock = null!;
    private AuthorizationFilterContext _authFilterCtx = null!;

    [OneTimeSetUp]
    public void SetUp() {
        _loggerMock = MockBase.Simulate<ILoggerService>().Object;
        MockConfiguration(out _configMock);
        MockAuthorizationFilterContext(out _authFilterCtx, new KeyValuePair<string, string>(nameof(HttpHeaderKeys.TwoFactorToken), "4ec503be252d765ea37621a629afdaa6"));
    }

    [TearDown]
    public void TearDown() {
        MockConfiguration(out _configMock);
        MockAuthorizationFilterContext(out _authFilterCtx, new KeyValuePair<string, string>(nameof(HttpHeaderKeys.TwoFactorToken), "4ec503be252d765ea37621a629afdaa6"));
        _authFilterCtx.Result = null;
    }

    [Test]
    public void Test_OnAuthorization_TFADisabled() {
        MockConfiguration(out _configMock, false);

        var tfaAuthorize = new TwoFactorAuthorize(MockBase.EcosystemMock, _loggerMock, _configMock);
        tfaAuthorize.OnAuthorization(_authFilterCtx);

        var result = _authFilterCtx.Result as ErrorResponse;
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public void Test_OnAuthorization_TFANoSecretKey() {
        var tfaAuthorize = new TwoFactorAuthorize(MockBase.EcosystemMock, _loggerMock, _configMock);
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
        
        var tfaAuthorize = new TwoFactorAuthorize(MockBase.EcosystemMock, _loggerMock, _configMock);
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
        var tfaAuthorize = new TwoFactorAuthorize(MockBase.EcosystemMock, _loggerMock, _configMock);
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
        var tfaAuthorize = new TwoFactorAuthorize(MockBase.EcosystemMock, _loggerMock, _configMock);
        tfaAuthorize.OnAuthorization(_authFilterCtx);
    
        var result = _authFilterCtx.Result as ErrorResponse;
        Assert.That(result, Is.Null);
    }

    private static void MockConfiguration(out IConfiguration configMock, bool tfaEnabled = true) =>
        configMock = ConfigurationMock.Instance(new KeyValuePair<string, string>(
            $"{nameof(HalogenOptions)}{Constants.Colon}{MockBase.EcosystemMock.GetEnvironment()}{Constants.Colon}{nameof(HalogenOptions.Local.ServiceSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.ServiceSettings.TwoFactorEnabled)}",
            tfaEnabled ? "True" : "False"
        ));

    private static void MockAuthorizationFilterContext(out AuthorizationFilterContext authFilterCtx, KeyValuePair<string, string> keyVal) {
        var (key, val) = keyVal;
        var requestHeaders = new Dictionary<string, StringValues> {{ key, val }};
        authFilterCtx = AuthorizationFilterContextMock.Actual([
            new KeyValuePair<string, Dictionary<string, StringValues>>(nameof(HttpContext.Request.Headers), requestHeaders),
        ]);
    }
}
