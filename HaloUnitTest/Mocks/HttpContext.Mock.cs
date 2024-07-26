using HelperLibrary.Shared.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;

namespace HaloUnitTest.Mocks;

internal sealed class HttpContextMock: MockBase {

    private static readonly Lazy<HttpContextMock> HttpCtxMock = new(() => new HttpContextMock());

    private readonly Mock<HttpContext> _httpContextMock;

    private HttpContextMock() {
        _httpContextMock = Simulate<HttpContext>();
    }

    internal static Mock<HttpContext> Instance() => HttpCtxMock.Value._httpContextMock;

    internal static HttpContext Instance<T>(string key, T val) {
        var httpContextMock = Instance();
        switch (key) {
            case nameof(HttpContext.Request.Headers):
                var headerDictionary = new HeaderDictionary((Dictionary<string, StringValues>)(object)val!);
                httpContextMock.SetupGet(m => m.Request.Headers).Returns(headerDictionary);
                break;
            case nameof(HttpContext.Request.Body):
                var bodyStream = val.ToStream();
                httpContextMock.SetupGet(m => m.Request.Body).Returns(bodyStream);
                break;
        }

        return httpContextMock.Object;
    }
}
