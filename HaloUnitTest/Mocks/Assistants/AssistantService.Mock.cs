using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Services;
using Moq;

namespace HaloUnitTest.Mocks.Assistants;

// Singleton
internal sealed class AssistantServiceMock: MockBase {

    private static readonly Lazy<AssistantServiceMock> AssistantSvMock = new(() => new AssistantServiceMock());

    private readonly Mock<AssistantService> _assistantSvMock;

    private AssistantServiceMock() {
        _assistantSvMock = Simulate<AssistantService>();
    }

    internal static Mock<AssistantService> Instance() => AssistantSvMock.Value._assistantSvMock;

    internal static AssistantService Instance<T>(KeyValuePair<string, T>[] vals) {
        var assistantSvMock = Instance();
        
        foreach (var (key, val) in vals)
            switch (key) {
                case nameof(IAssistantService.IsHumanActivity):
                    assistantSvMock.Setup(m => m.IsHumanActivity(It.IsAny<string>())).ReturnsAsync((RecaptchaResponse)(object)val!);
                    break;
                case nameof(IAssistantService.GenerateQrImage):
                    assistantSvMock.Setup(m => m.GenerateQrImage(It.IsAny<string>(), null)).Returns((byte[])(object)val!);
                    break;
            }

        return assistantSvMock.Object;
    }
}
