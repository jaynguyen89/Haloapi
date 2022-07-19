using AssistantLibrary.Bindings;

namespace AssistantLibrary.Interfaces; 

internal interface IAssistantService {

    Task<RecaptchaResponse?> IsHumanActivity(string clientToken);
}