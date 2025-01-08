using AssistantLibrary.Bindings;

namespace AssistantLibrary.Interfaces; 

public interface ICryptoService {

    /// <summary>
    /// To generate the random Hash and Salt strings to encrypt the plain texts.
    /// </summary>
    /// <param name="plainText">string</param>
    /// <param name="saltLength">int</param>
    /// <returns>KeyValuePair:string:string</returns>
    KeyValuePair<string, string> GenerateHashAndSalt(string plainText, int saltLength);

    /// <summary>
    /// To get a random string to use used as Salt for encryption.
    /// </summary>
    /// <param name="length">int</param>
    /// <returns>string</returns>
    string GetSalt(int length);

    /// <summary>
    /// To check the Hashed string against the plain text if they are originally the same.
    /// </summary>
    /// <param name="hash">string</param>
    /// <param name="plainText">string</param>
    /// <returns>bool</returns>
    bool IsHashMatchesPlainText(string hash, string plainText);

    /// <summary>
    /// To generate a random RSA key pair.
    /// </summary>
    /// <returns>RsaKeyPair</returns>
    Task<RsaKeyPair> GenerateRsaKeyPair();

    /// <summary>
    /// To generate the signature of the plain text using an RSA key pair.
    /// </summary>
    /// <param name="plainText">string</param>
    /// <param name="keyPair">RsaKeyPair</param>
    /// <returns>string?</returns>
    string? RsaSign(string plainText, RsaKeyPair keyPair);

    /// <summary>
    /// To check the signature against the plain text if they are originally the same.
    /// </summary>
    /// <param name="signature">string</param>
    /// <param name="plainText">string</param>
    /// <param name="keyPair">RsaKeyPair</param>
    /// <returns>bool?</returns>
    bool? RsaVerifySignature(string signature, string plainText, RsaKeyPair keyPair);

    /// <summary>
    /// To hash the plain text using SHA512 algorithm.
    /// </summary>
    /// <param name="plainText">string</param>
    /// <returns>string</returns>
    Task<string> CreateSha512Hash(string plainText);

    /// <summary>
    /// To hash the plain text using HMAC-SHA512 algorithm.
    /// </summary>
    /// <param name="plainText">string</param>
    /// <returns>string</returns>
    Task<string> CreateHmacSha521Hash(string plainText);

    /// <summary>
    /// To hash the plain text using HMAC-MD5 algorithm.
    /// </summary>
    /// <param name="plainText">string</param>
    /// <returns>string</returns>
    Task<string> CreateHmacMd5Hash(string plainText);
}