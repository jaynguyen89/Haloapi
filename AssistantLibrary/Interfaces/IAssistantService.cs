using AssistantLibrary.Bindings;

namespace AssistantLibrary.Interfaces; 

public interface IAssistantService {

    Task<RecaptchaResponse?> IsHumanActivity(string clientToken);
}