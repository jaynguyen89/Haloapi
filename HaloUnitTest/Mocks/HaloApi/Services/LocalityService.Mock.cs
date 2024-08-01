using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;
using Moq;

namespace HaloUnitTest.Mocks.HaloApi.Services;

// Singleton
internal sealed class LocalityServiceMock: MockBase {

    private static readonly Lazy<LocalityServiceMock> LocalitySvMock = new(() => new LocalityServiceMock());

    private readonly Mock<LocalityService> _localitySvMock;

    private LocalityServiceMock() {
        _localitySvMock = Simulate<LocalityService>();
    }

    internal static Mock<LocalityService> Instance() => LocalitySvMock.Value._localitySvMock;

    internal static LocalityService Instance<T>(string propertyName, T[] returnVal) {
        var localitySvMock = Instance();
        switch (propertyName) {
            case nameof(ILocalityService.GetTelephoneCodes):
                localitySvMock.Setup(m => m.GetTelephoneCodes()).ReturnsAsync(Array.ConvertAll(returnVal, val => (string)(object)val!));
                break;
            case nameof(ILocalityService.GetLocalitiesForPublicData):
                localitySvMock.Setup(m => m.GetLocalitiesForPublicData()).ReturnsAsync(Array.ConvertAll(returnVal, val => (Locality)(object)val!));
                break;
        }

        return localitySvMock.Object;
    }
}
