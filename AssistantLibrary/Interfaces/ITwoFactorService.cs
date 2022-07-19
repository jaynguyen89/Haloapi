using AssistantLibrary.Bindings;

namespace AssistantLibrary.Interfaces; 

public interface ITwoFactorService {

    TwoFactorData GetTwoFactorAuthenticationData(in GetTwoFactorBinding binding);

    bool VerifyTwoFactorAuthenticationPin(in VerifyTwoFactorBinding binding);
}