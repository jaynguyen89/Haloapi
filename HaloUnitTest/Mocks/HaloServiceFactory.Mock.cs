using Halogen.Auxiliaries.Interfaces;
using Halogen.Services;
using HelperLibrary.Shared;
using Moq;

namespace HaloUnitTest.Mocks;

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
        foreach (var val in vals)
            haloServiceFactoryMock.Setup(m => m.GetService<T>(val.Key)).Returns(val.Value);
        
        return haloServiceFactoryMock.Object;
    }
}
