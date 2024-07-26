using Halogen.Services.AppServices.Services;
using Moq;

namespace HaloUnitTest.Mocks;

internal sealed class SessionServiceMock: MockBase {

    private static readonly Lazy<SessionServiceMock> SessionSvMock = new(() => new SessionServiceMock());

    private readonly Mock<SessionService> _sessionSvMock;

    private SessionServiceMock() {
        _sessionSvMock = Simulate<SessionService>();
    }

    internal static Mock<SessionService> Instance() => SessionSvMock.Value._sessionSvMock;

    internal static SessionService Instance<T>(T[] vals) {
        var sessionSvMock = Instance();
        foreach (var val in vals)
            sessionSvMock.Setup(m => m.Get<T>(nameof(T))).Returns(val);

        return sessionSvMock.Object;
    }
}
