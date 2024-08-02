using System.Net;
using Halogen.Attributes;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings.ServiceBindings;
using Halogen.Bindings.ViewModels;
using HaloUnitTest.Mocks;
using HaloUnitTest.Mocks.HaloApi.Auxiliaries;
using HaloUnitTest.Mocks.HaloApi.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Authorization = Halogen.Bindings.ServiceBindings.Authorization;

namespace HaloUnitTest.HalogenApiTests.Attributes;

[TestFixture]
public sealed class AuthenticatedAuthorizeTest {

    private ILoggerService _loggerMock;
    private Authorization _authorization = null!;
    private AuthorizationFilterContext _authorizationFilterCtx;

    [OneTimeSetUp]
    public void SetUp() {
        _loggerMock = MockBase.Simulate<ILoggerService>().Object;
        MockAuthorization(out _authorization);
        
        var requestHeaders = new Dictionary<string, StringValues> {
            { nameof(HttpHeaderKeys.AccountId), "2bda928a540d412e8297b2c880eb8ef0" },
            { nameof(HttpHeaderKeys.Authorization), "Bearer 3fc9b689459d738f8c88a3a48aa9e33542016b7a4052e001aaa536fca74813cb" },
            { nameof(HttpHeaderKeys.AuthorizationToken), "f41f3fa625ff120ddca7ef456bf66371ecea23c129f4e4c32367101edb516cf8" },
        };
        _authorizationFilterCtx = AuthorizationFilterContextMock.Actual([
            new KeyValuePair<string, Dictionary<string, StringValues>>(nameof(HttpContext.Request.Headers), requestHeaders),
        ]);
    }

    [TearDown]
    public void TearDown() => MockAuthorization(out _authorization);

    [Test]
    public void Test_OnAuthorization_Failure_AccountId() {
        _authorization.AccountId = "something";
        var haloSvFactoryMock = MockHaloServiceFactory(_authorization);
        
        var authenticatedAuthorize = new AuthenticatedAuthorize(_loggerMock, haloSvFactoryMock);
        authenticatedAuthorize.OnAuthorization(_authorizationFilterCtx);

        var result = _authorizationFilterCtx.Result as ErrorResponse;
        Assert.That(result, Is.Not.Null);

        var expect = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.InvalidUser.GetValue()}");
        Assert.Multiple(() => {
            Assert.That(result.StatusCode, Is.EqualTo(expect.StatusCode));
            Assert.That(result.Content, Is.EqualTo(expect.Content));
        });
    }
    
    [Test]
    public void Test_OnAuthorization_Failure_BearerToken() {
        _authorization.BearerToken = "Bearer something";
        var haloSvFactoryMock = MockHaloServiceFactory(_authorization);
        
        var authenticatedAuthorize = new AuthenticatedAuthorize(_loggerMock, haloSvFactoryMock);
        authenticatedAuthorize.OnAuthorization(_authorizationFilterCtx);

        var result = _authorizationFilterCtx.Result as ErrorResponse;
        Assert.That(result, Is.Not.Null);

        var expect = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.MismatchedBearerToken.GetValue()}");
        Assert.Multiple(() => {
            Assert.That(result.StatusCode, Is.EqualTo(expect.StatusCode));
            Assert.That(result.Content, Is.EqualTo(expect.Content));
        });
    }
    
    [Test]
    public void Test_OnAuthorization_Failure_AuthToken() {
        _authorization.AuthorizationToken = "something";
        var haloSvFactoryMock = MockHaloServiceFactory(_authorization);
        
        var authenticatedAuthorize = new AuthenticatedAuthorize(_loggerMock, haloSvFactoryMock);
        authenticatedAuthorize.OnAuthorization(_authorizationFilterCtx);

        var result = _authorizationFilterCtx.Result as ErrorResponse;
        Assert.That(result, Is.Not.Null);

        var expect = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.MismatchedAuthToken.GetValue()}");
        Assert.Multiple(() => {
            Assert.That(result.StatusCode, Is.EqualTo(expect.StatusCode));
            Assert.That(result.Content, Is.EqualTo(expect.Content));
        });
    }
    
    [Test]
    public void Test_OnAuthorization_Failure_AuthTimestamp() {
        _authorization.AuthorizedTimestamp = DateTime.UtcNow.AddHours(-2.1).ToTimestamp();
        var haloSvFactoryMock = MockHaloServiceFactory(_authorization);
        
        var authenticatedAuthorize = new AuthenticatedAuthorize(_loggerMock, haloSvFactoryMock);
        authenticatedAuthorize.OnAuthorization(_authorizationFilterCtx);

        var result = _authorizationFilterCtx.Result as ErrorResponse;
        Assert.That(result, Is.Not.Null);

        var expect = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.AuthorizationExpired.GetValue()}");
        Assert.Multiple(() => {
            Assert.That(result.StatusCode, Is.EqualTo(expect.StatusCode));
            Assert.That(result.Content, Is.EqualTo(expect.Content));
        });
    }

    [Test]
    public void Test_OnAuthorization_Success() {
        var haloSvFactoryMock = MockHaloServiceFactory(_authorization);
        
        var authenticatedAuthorize = new AuthenticatedAuthorize(_loggerMock, haloSvFactoryMock);
        authenticatedAuthorize.OnAuthorization(_authorizationFilterCtx);

        var result = _authorizationFilterCtx.Result as ErrorResponse;
        Assert.That(result, Is.Null);
    }
    
    private static void MockAuthorization(out Authorization auth) => auth = new() {
        AccountId = "2bda928a540d412e8297b2c880eb8ef0",
        Roles = [Enums.Role.Customer],
        BearerToken = "3fc9b689459d738f8c88a3a48aa9e33542016b7a4052e001aaa536fca74813cb",
        AuthorizationToken = "f41f3fa625ff120ddca7ef456bf66371ecea23c129f4e4c32367101edb516cf8",
        RefreshToken = string.Empty,
        AuthorizedTimestamp = DateTime.UtcNow.ToTimestamp(),
        ValidityDuration = 2.ToMilliseconds(Enums.TimeUnit.Hour),
    };

    private static IHaloServiceFactory MockHaloServiceFactory(Authorization auth) {
        var sessionSvMock = SessionServiceMock.Instance<Authorization>([auth]);
        
        var haloSvFactoryMock = HaloServiceFactoryMock.Instance<object>([
            new KeyValuePair<Enums.ServiceType, object>(Enums.ServiceType.AppService, sessionSvMock),
        ]);

        return haloSvFactoryMock;
    }
}
