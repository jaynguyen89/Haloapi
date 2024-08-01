using Halogen.Auxiliaries.Interfaces;
using HelperLibrary.Shared;
using Moq;

namespace HaloUnitTest.Mocks.HaloApi.Auxiliaries;

// Singleton
internal sealed class HaloServiceFactoryMock: MockBase {

    private static readonly Lazy<HaloServiceFactoryMock> HaloSFMock = new(() => new HaloServiceFactoryMock());
    
    private readonly Mock<IHaloServiceFactory> _haloServiceFactory;

    private HaloServiceFactoryMock() {
        _haloServiceFactory = Simulate<IHaloServiceFactory>();
    }

    internal static Mock<IHaloServiceFactory> Instance() => HaloSFMock.Value._haloServiceFactory;

    internal static IHaloServiceFactory Instance<T>(KeyValuePair<Enums.ServiceType, T>[] vals) {
        var haloServiceFactoryMock = Instance();
        foreach (var (key, val) in vals)
            haloServiceFactoryMock.Setup(m => m.GetService<T>(key)).Returns(val);
        
        return haloServiceFactoryMock.Object;
    }
}
