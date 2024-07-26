using System.Net;
using Halogen.Attributes;
using Halogen.Auxiliaries.Interfaces;
using Halogen.Bindings.ViewModels;
using HaloUnitTest.Mocks;
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
    private Authorization _authorization;
    private Dictionary<string, StringValues> _headers;

    [SetUp]
    public void SetUp() {
        _loggerMock = MockBase.Simulate<ILoggerService>().Object;
        _authorization = new Authorization {
            AccountId = "2bda928a540d412e8297b2c880eb8ef0",
            Roles = [Enums.Role.Customer],
            BearerToken = "3fc9b689459d738f8c88a3a48aa9e33542016b7a4052e001aaa536fca74813cb",
            AuthorizationToken = "f41f3fa625ff120ddca7ef456bf66371ecea23c129f4e4c32367101edb516cf8",
            RefreshToken = "",
            AuthorizedTimestamp = DateTime.UtcNow.ToTimestamp(),
            ValidityDuration = 2.ToMilliseconds(Enums.TimeUnit.Hour),
        };
        _headers = new Dictionary<string, StringValues> {
            { "AccountId", "2bda928a540d412e8297b2c880eb8ef0" },
            { "Authorization", "Bearer 3fc9b689459d738f8c88a3a48aa9e33542016b7a4052e001aaa536fca74813cb" },
            { "AuthorizationToken", "f41f3fa625ff120ddca7ef456bf66371ecea23c129f4e4c32367101edb516cf8" },
        };
    }

    [Test]
    public void Test_OnAuthorization_AccountId() {
        _authorization.AccountId = "2097939e8322471ba14aa3779441b5db";
        var haloSvFactoryMock = MockHaloServiceFactory(_authorization);
        var authenticatedAuthorize = new AuthenticatedAuthorize(_loggerMock, haloSvFactoryMock);

        var httpContext = HttpContextMock.Instance(nameof(HttpContext.Request.Headers), _headers);
        var authorizationFilterContext = AuthorizationFilterContextMock.Instance([
            new KeyValuePair<string, HttpContext>(nameof(AuthorizationFilterContext.HttpContext), httpContext),
        ]);
        
        authenticatedAuthorize.OnAuthorization(authorizationFilterContext);

        var result = authorizationFilterContext.Result as ErrorResponse;
        Assert.That(result, Is.Not.Null);

        var expect = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(AuthenticatedAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.InvalidUser.GetValue()}");
        Assert.Multiple(() => {
            Assert.That(result.StatusCode, Is.EqualTo(expect.StatusCode));
            Assert.That(result.Content, Is.EqualTo(expect.Content));
        });
    }
    
    [Test]
    public void Test_OnAuthorization_BearerToken() {
        
    }
    
    [Test]
    public void Test_OnAuthorization_AuthToken() {
        
    }
    
    [Test]
    public void Test_OnAuthorization_AuthTimestamp() {
        
    }

    private static IHaloServiceFactory MockHaloServiceFactory(Authorization auth) {
        var sessionSvMock = SessionServiceMock.Instance<Authorization>([auth]);
        
        var haloSvFactoryMock = HaloServiceFactoryMock.Instance<object>([
            new KeyValuePair<Enums.ServiceType, object>(Enums.ServiceType.AppService, sessionSvMock),
        ]);

        return haloSvFactoryMock;
    }
}
