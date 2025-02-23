using HelperLibrary.Shared.Ecosystem;
using Moq;
using Moq.AutoMock;

namespace HaloUnitTest.Mocks;

internal class MockBase
{
    internal const string Environment = "NUnit";

    internal static readonly IEcosystem EcosystemMock = new Ecosystem {
        Environment = Environment,
        UseLongerId = false,
        ServerSetting = new Ecosystem.ServerSettings {
            AwsAccessKeyId = string.Empty,
            AwsSecretAccessKey = string.Empty,
            AwsRegion = string.Empty,
        },
    };
    
    private AutoMocker Mocker { get; }

    internal MockBase() {
        Mocker = new AutoMocker();
    }

    /// <summary>
    /// Returns Moq.Mock.Of()
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>T</returns>
    internal static T Forge<T>() where T: class {
        return Mock.Of<T>();
    }

    /// <summary>
    /// Let AutoMocker consume Moq: new AutoMocker().Use(Moq.Mock.Of())
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal void Synthesize<T>() where T: class {
        Mocker.Use(Forge<T>());
    }

    /// <summary>
    /// Returns new AutoMocker().CreateInstance()
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>T</returns>
    internal T Create<T>() where T: class => Mocker.CreateInstance<T>();

    /// <summary>
    /// Returns new AutoMock().Get()
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>T</returns>
    internal T Get<T>() where T: class => Mocker.Get<T>();

    /// <summary>
    /// Returns new AutoMocker().GetMock()
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>Mock:T</returns>
    internal Mock<T> Obtain<T>() where T: class => Mocker.GetMock<T>();

    /// <summary>
    /// Returns new Moq.Mock()
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>Mock:T</returns>
    internal static Mock<T> Simulate<T>() where T: class => new();
}
