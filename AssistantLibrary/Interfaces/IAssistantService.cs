using AssistantLibrary.Bindings;

namespace AssistantLibrary.Interfaces; 

public interface IAssistantService {

    /// <summary>
    /// To verify the client Google Recaptcha token.
    /// </summary>
    /// <param name="clientToken">string</param>
    /// <returns>RecaptchaResponse</returns>
    Task<RecaptchaResponse> IsHumanActivity(string clientToken);

    /// <summary>
    /// To generate a QR image for client to register with Google Authenticator app.
    /// </summary>
    /// <param name="information">string</param>
    /// <param name="image">FileStream?</param>
    /// <returns>byte[]</returns>
    byte[] GenerateQrImage(string information, FileStream? image);
}