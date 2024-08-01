using AssistantLibrary;
using Moq;

namespace HaloUnitTest.Mocks.Assistants;

internal sealed class AssistantServiceFactoryMock: MockBase {

    private static readonly Lazy<AssistantServiceFactoryMock> AssistantSFMock = new(() => new AssistantServiceFactoryMock());

    private readonly Mock<IAssistantServiceFactory> _assistantServiceFactory;

    private AssistantServiceFactoryMock() {
        _assistantServiceFactory = Simulate<IAssistantServiceFactory>();
    }

    internal static Mock<IAssistantServiceFactory> Instance() => AssistantSFMock.Value._assistantServiceFactory;

    internal static IAssistantServiceFactory Instance<T>(T[] vals) {
        var assistantServiceFactory = Instance();
        foreach (var val in vals)
            assistantServiceFactory.Setup(m => m.GetService<T>()).Returns(val);

        return assistantServiceFactory.Object;
    }
}
