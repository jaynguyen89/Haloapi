using Moq;
using Moq.AutoMock;

namespace HaloUnitTest.Mocks;

internal class MockBase {
    private AutoMocker Mocker { get; }

    internal MockBase() {
        Mocker = new AutoMocker();
    }

    internal static T Forge<T>() where T: class {
        return Mock.Of<T>();
    }

    internal void Synthesize<T>() where T: class {
        Mocker.Use(Forge<T>());
    }

    internal T Create<T>() where T: class => Mocker.CreateInstance<T>();

    internal T Get<T>() where T: class => Mocker.Get<T>();

    internal Mock<T> Obtain<T>() where T: class => Mocker.GetMock<T>();

    internal static Mock<T> Simulate<T>() where T: class => new();
}
