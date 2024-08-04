using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Services;
using Moq;

namespace HaloUnitTest.Mocks.Assistants;

internal sealed class TwoFactorServiceMock: MockBase {

    private static readonly Lazy<TwoFactorServiceMock> TwoFactorSvMock = new(() => new TwoFactorServiceMock());

    private readonly Mock<TwoFactorService> _twoFactorServiceMock;

    private TwoFactorServiceMock() {
        _twoFactorServiceMock = Simulate<TwoFactorService>();
    }

    internal static Mock<TwoFactorService> Instance() => TwoFactorSvMock.Value._twoFactorServiceMock;

    internal static TwoFactorService Instance<T, TK>(string propertyName, T param, TK val) {
        var twoFactorServiceMock = Instance();
        switch (propertyName) {
            case nameof(ITwoFactorService.VerifyTwoFactorAuthenticationPin):
                twoFactorServiceMock.Setup(m => m.VerifyTwoFactorAuthenticationPin((VerifyTwoFactorBinding)(object)param!)).Returns(bool.Parse((string)(object)val!));
                break;
            case nameof(ITwoFactorService.GetTwoFactorAuthenticationData):
                twoFactorServiceMock.Setup(m => m.GetTwoFactorAuthenticationData((GetTwoFactorBinding)(object)param!)).Returns((TwoFactorData)(object)val!);
                break;
        }

        return twoFactorServiceMock.Object;
    }
}