using AssistantLibrary.Bindings;

namespace AssistantLibrary.Interfaces; 

public interface IAssistantService {

    Task<RecaptchaResponse> IsHumanActivity(string clientToken);

    byte[] GenerateQrImage(string information, FileStream? image);
}