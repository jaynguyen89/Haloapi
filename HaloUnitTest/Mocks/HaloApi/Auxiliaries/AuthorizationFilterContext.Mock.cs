using HelperLibrary.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace HaloUnitTest.Mocks.HaloApi.Auxiliaries;

// Example usage:
// var authorizationFilterContext = AuthorizationFilterContextMock.Instance([
//     new KeyValuePair<string, HttpContext>(nameof(AuthorizationFilterContext.HttpContext), httpContext),
// ]);

// Singleton
[Problematic("This mock's Instance() and Instance<T>() is unable to do SetupGet or Setup because no property in AuthorizationFilterContext is overridable.")]
[Attention("Due to the described problem, only use the Actual<T>() method for Test Fixtures.")]
[Todo("Extend the Actual<T>() method switch-case to set up mock data for other HttpContext properties.")]
internal sealed class AuthorizationFilterContextMock: MockBase {

    private static readonly Lazy<AuthorizationFilterContextMock> AuthFilterCtxMock = new(() => new AuthorizationFilterContextMock());

    private readonly Mock<AuthorizationFilterContext> _authFilterCtxMock;

    private AuthorizationFilterContextMock() {
        _authFilterCtxMock = Simulate<AuthorizationFilterContext>();
    }

    internal static Mock<AuthorizationFilterContext> Instance() => AuthFilterCtxMock.Value._authFilterCtxMock;

    internal static AuthorizationFilterContext Instance<T>(KeyValuePair<string, T>[] keyVals) {
        var authFilterCtxMock = Instance();
        foreach (var (key, val) in keyVals)
            switch (key) {
                case nameof(AuthorizationFilterContext.HttpContext):
                    authFilterCtxMock.SetupGet(m => m.HttpContext).Returns((HttpContext)(object)val!);
                    break;
            }

        return authFilterCtxMock.Object;
    }

    internal static AuthorizationFilterContext Actual<T>(KeyValuePair<string, T>[] keyVals) {
        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        
        foreach (var (key, val) in keyVals) {
            actionContext.HttpContext = key switch {
                nameof(HttpContext.Request.Headers) => HttpContextMock.Instance(nameof(HttpContext.Request.Headers), val),
                _ => actionContext.HttpContext,
            };
        }

        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }
}
