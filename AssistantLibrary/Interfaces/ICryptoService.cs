using AssistantLibrary.Bindings;

namespace AssistantLibrary.Interfaces; 

public interface ICryptoService {

    KeyValuePair<string, string> GenerateHashAndSalt(string plainText, int saltLength);

    string GetSalt(int length);

    bool IsHashMatchesPlainText(string hash, string plainText);

    Task<RsaKeyPair> GenerateRsaKeyPair();

    string? RsaSign(string plainText, RsaKeyPair keyPair);

    bool? RsaVerifySignature(string signature, string plainText, RsaKeyPair keyPair);
}