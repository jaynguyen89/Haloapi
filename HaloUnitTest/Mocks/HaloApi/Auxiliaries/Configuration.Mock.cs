using Microsoft.Extensions.Configuration;
using Moq;

namespace HaloUnitTest.Mocks.HaloApi.Auxiliaries;

// Singleton
internal sealed class ConfigurationMock: MockBase {

    private static readonly Lazy<ConfigurationMock> ConfigMock = new(() => new ConfigurationMock());

    private readonly Mock<IConfiguration> _configMock;

    private ConfigurationMock() {
        _configMock = Simulate<IConfiguration>();
    }

    internal static Mock<IConfiguration> Instance() => ConfigMock.Value._configMock;

    internal static IConfiguration Instance(KeyValuePair<string, string> val, bool keyless = false) {
        var configMock = Instance();
        
        var configSectionMock = Simulate<IConfigurationSection>();
        configSectionMock.Setup(m => m.Value).Returns(val.Value);

        configMock.Setup(m => m.GetSection(keyless ? It.IsAny<string>() : val.Key)).Returns(configSectionMock.Object);
        return configMock.Object;
    }
}
