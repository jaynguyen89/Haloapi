using AssistantLibrary;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Services;
using HaloUnitTest.Mocks;
using HaloUnitTest.Mocks.Assistants;
using HaloUnitTest.Mocks.HaloApi.Auxiliaries;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Configuration;

namespace HaloUnitTest.HalogenApiTests.Attributes;

[TestFixture]
public sealed class RecaptchaAuthorizeTest {
    
    private ILoggerService _loggerMock;
    private IConfiguration _configMock;
    private IAssistantServiceFactory _assistantSvFactoryMock;

    [OneTimeSetUp]
    public void SetUp() {
        _loggerMock = MockBase.Simulate<ILoggerService>().Object;
        _configMock = ConfigurationMock.Instance(new KeyValuePair<string, string>("any", "True"), true);

        var recaptchaResponse = new RecaptchaResponse {
            IsHuman = true,
            HostName = "NUnit"
        };
        var assistantSvMock = AssistantServiceMock.Instance<RecaptchaResponse>([new KeyValuePair<string, RecaptchaResponse>(nameof(IAssistantService.IsHumanActivity), recaptchaResponse)]);
        _assistantSvFactoryMock = AssistantServiceFactoryMock.Instance<AssistantService>([assistantSvMock]);
    }

    [TearDown]
    public void TearDown() {
        
    }

    [Test]
    public void Test_OnAuthorization_RecaptchaDisabled() {
        
    }
    
    [Test]
    public void Test_OnAuthorization_RecaptchaFailure() {
        
    }
    
    [Test]
    public void Test_OnAuthorization_RecaptchaSuccess() {
        
    }
}
