using HelperLibrary.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;

namespace HaloUnitTest.Mocks.HaloApi.Auxiliaries;

// Example usage:
// var authorizationFilterContext = AuthorizationFilterContextMock.Instance([
//     new KeyValuePair<string, HttpContext>(nameof(AuthorizationFilterContext.HttpContext), httpContext),
// ]);

// Singleton
[Problematic("This mock is unable to do SetupGet or Setup because no property in AuthorizationFilterContext is overridable.")]
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
}
