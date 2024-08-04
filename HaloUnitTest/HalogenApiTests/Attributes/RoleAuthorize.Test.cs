using System.Net;
using Halogen.Attributes;
using Halogen.Bindings.ViewModels;
using HaloUnitTest.Mocks.HaloApi.Auxiliaries;
using HelperLibrary.Attributes;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Authorization = Halogen.Bindings.ServiceBindings.Authorization;

namespace HaloUnitTest.HalogenApiTests.Attributes;

[Problematic("See comment on methos MockAuthorizationFilterContext.")]
[TestFixture]
public sealed class RoleAuthorizeTest {
    
    private Authorization _authorization = new() {
        AccountId = "2bda928a540d412e8297b2c880eb8ef0",
        Roles = [Enums.Role.Customer],
        BearerToken = "3fc9b689459d738f8c88a3a48aa9e33542016b7a4052e001aaa536fca74813cb",
        AuthorizationToken = "f41f3fa625ff120ddca7ef456bf66371ecea23c129f4e4c32367101edb516cf8",
        RefreshToken = string.Empty,
        AuthorizedTimestamp = DateTime.UtcNow.ToTimestamp(),
        ValidityDuration = 2.ToMilliseconds(Enums.TimeUnit.Hour),
    };

    [Test]
    public void Test_OnAuthorization_InvalidUser() {
        var authFilterCtx = MockAuthorizationFilterContext(null);

        var roleAuthorize = new RoleAuthorize(Enums.Role.Customer);
        roleAuthorize.OnAuthorization(authFilterCtx);

        var result = authFilterCtx.Result as ErrorResponse;
        Assert.That(result, Is.Not.Null);

        var expect = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(RoleAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.InvalidUser.GetValue()}");
        Assert.Multiple(() => {
            Assert.That(result.StatusCode, Is.EqualTo(expect.StatusCode));
            Assert.That(result.Content, Is.EqualTo(expect.Content));
        });
    }

    [TestCase("Customer")]
    [TestCase("Customer,Staff")]
    public void Test_OnAuthorization_InvalidRole(string roles) {
        var enumRoles = roles.Split(",").Select(role => (Enums.Role)role.ToEnum<Enums.Role>()!).ToArray();
        _authorization.Roles = [Enums.Role.Supplier];
        
        var authFilterCtx = MockAuthorizationFilterContext(_authorization);
        var roleAuthorize = new RoleAuthorize(enumRoles);
        roleAuthorize.OnAuthorization(authFilterCtx);
        
        var result = authFilterCtx.Result as ErrorResponse;
        Assert.That(result, Is.Not.Null);

        var expect = new ErrorResponse(HttpStatusCode.Unauthorized, $"{nameof(RoleAuthorize)}{Constants.FSlash}{Enums.AuthorizationFailure.InvalidRole.GetValue()}");
        Assert.Multiple(() => {
            Assert.That(result.StatusCode, Is.EqualTo(expect.StatusCode));
            Assert.That(result.Content, Is.EqualTo(expect.Content));
        });
    }

    [TestCase("Customer,Supplier")]
    [TestCase("Supplier,Administrator")]
    public void Test_OnAuthorization_Success(string roles) {
        var enumRoles = roles.Split(",").Select(role => (Enums.Role)role.ToEnum<Enums.Role>()!).ToArray();
        _authorization.Roles = [Enums.Role.Supplier, Enums.Role.Moderator];
        
        var authFilterCtx = MockAuthorizationFilterContext(_authorization);
        var roleAuthorize = new RoleAuthorize(enumRoles);
        roleAuthorize.OnAuthorization(authFilterCtx);
        
        var result = authFilterCtx.Result as ErrorResponse;
        Assert.That(result, Is.Null);
    }

    [Problematic("Need to fix: The sessionMock doesn't set session item.")]
    private static AuthorizationFilterContext MockAuthorizationFilterContext(Authorization? auth) {
       var sessionMock = new DistributedSession(
            new RedisCache(new RedisCacheOptions()),
            "1af17e73721dbe0c40011b82ed4bb1a7dbe3ce29",
            TimeSpan.MinValue,
            TimeSpan.MaxValue,
            () => true,
            new LoggerFactory(),
            true
        );
        
        sessionMock.SetString(nameof(Authorization), JsonConvert.SerializeObject(auth));
        
        return AuthorizationFilterContextMock.Actual([
            new KeyValuePair<string, ISession>(nameof(HttpContext.Session), sessionMock),
        ]);
    }
}
