using AssistantLibrary.Bindings;

namespace AssistantLibrary.Interfaces; 

public interface ITwoFactorService {

    /// <summary>
    /// To generate the code to use for registering Google Authenticator app.
    /// </summary>
    /// <param name="binding">GetTwoFactorBinding</param>
    /// <returns>TwoFactorData</returns>
    TwoFactorData GetTwoFactorAuthenticationData(in GetTwoFactorBinding binding);

    /// <summary>
    /// To verify the Two-Factor PIN from client (given by the Google Authenticator app).
    /// </summary>
    /// <param name="binding">VerifyTwoFactorBinding</param>
    /// <returns>bool</returns>
    bool VerifyTwoFactorAuthenticationPin(in VerifyTwoFactorBinding binding);
}