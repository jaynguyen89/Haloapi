using System.Security.Cryptography;
using System.Xml.Serialization;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using BCrypt;
using HelperLibrary;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Configuration;

namespace AssistantLibrary.Services; 

public sealed class CryptoService: ServiceBase, ICryptoService {

    private readonly int _rsaKeySize;

    public CryptoService(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ): base(ecosystem, logger, configuration) {
        var rsaKeySize = _configuration.AsEnumerable().Single(x => x.Key.Equals($"{_baseOptionKey}{nameof(AssistantLibraryOptions.Local.RsaKeyLength)}")).Value;
        _rsaKeySize = int.Parse(rsaKeySize);
    }
    
    public KeyValuePair<string, string> GenerateHashAndSalt(string plainText, int saltLength) {
        _logger.Log(new LoggerBinding<CryptoService> { Location = nameof(GenerateHashAndSalt) });
        
        var salt = GetSalt(saltLength);
        var hash = BCryptHelper.HashPassword(plainText, salt);

        return new KeyValuePair<string, string>(hash, salt);
    }

    public string GetSalt(int length) {
        _logger.Log(new LoggerBinding<CryptoService> { Location = nameof(GetSalt) });
        return BCryptHelper.GenerateSalt(length);
    }

    public bool IsHashMatchesPlainText(string hash, string plainText) {
        _logger.Log(new LoggerBinding<CryptoService> { Location = nameof(IsHashMatchesPlainText) });
        
        try {
            return BCryptHelper.CheckPassword(plainText, hash);
        }
        catch (SaltParseException) {
            return BCryptHelper.CheckPassword(hash, plainText);
        }
    }

    public async Task<RsaKeyPair> GenerateRsaKeyPair() {
        _logger.Log(new LoggerBinding<CryptoService> { Location = nameof(GenerateRsaKeyPair) });
        var rsaService = new RSACryptoServiceProvider(_rsaKeySize);

        var privateKeyParam = rsaService.ExportParameters(true);
        var publicKeyParam = rsaService.ExportParameters(false);

        var stringWriter = new StringWriter();
        var xmlSerializer = new XmlSerializer(typeof(RSAParameters));

        var keyPair = new RsaKeyPair();
        xmlSerializer.Serialize(stringWriter, privateKeyParam);
        keyPair.PrivateKey = stringWriter.ToString();

        await stringWriter.FlushAsync();
        xmlSerializer.Serialize(stringWriter, publicKeyParam);
        keyPair.PublicKey = stringWriter.ToString();

        await stringWriter.FlushAsync();
        stringWriter.Close();
        return keyPair;
    }

    public string? RsaSign(string plainText, RsaKeyPair keyPair) {
        _logger.Log(new LoggerBinding<CryptoService> { Location = nameof(RsaSign) });

        try {
            var xmlSerializer = new XmlSerializer(typeof(RSAParameters));
            var streamReader = new StreamReader(keyPair.PrivateKey);

            var publicKeyParam = (RSAParameters)xmlSerializer.Deserialize(streamReader)!;
            streamReader.Close();

            var rsaService = new RSACryptoServiceProvider();
            rsaService.ImportParameters(publicKeyParam);

            var cipherBytes = rsaService.Encrypt(plainText.EncodeDataUtf8(), false);
            return Convert.ToBase64String(cipherBytes);
        }
        catch {
            return default;
        }
    }

    public bool? RsaVerifySignature(string signature, string plainText, RsaKeyPair keyPair) {
        _logger.Log(new LoggerBinding<CryptoService> { Location = nameof(RsaVerifySignature) });

        try {
            var xmlSerializer = new XmlSerializer(typeof(RSAParameters));
            var streamReader = new StreamReader(keyPair.PublicKey);

            var privateKeyParam = (RSAParameters)xmlSerializer.Deserialize(streamReader)!;
            streamReader.Close();

            var rsaService = new RSACryptoServiceProvider();
            rsaService.ImportParameters(privateKeyParam);

            var decryptedBytes = rsaService.Decrypt(Convert.FromBase64String(signature), false);
            var decodedText = decryptedBytes.DecodeUtf8<string>();

            return plainText.Equals(decodedText);
        }
        catch {
            return default;
        }
    }

    [Obsolete("Derived cryptographic types are obsolete. Use the Create method on the base type instead.")]
    public async Task<string> CreateSha512Hash(string plainText) {
        _logger.Log(new LoggerBinding<CryptoService> { Location = nameof(CreateSha512Hash) });
        
        var plainTextStream = new MemoryStream();
        await plainTextStream.WriteAsync(plainText.EncodeDataUtf8());

        var sha512 = new SHA512Managed();
        var result = await sha512.ComputeHashAsync(plainTextStream);

        return result.DecodeUtf8<string>()!;
    }
    
    public async Task<string> CreateHmacSha521Hash(string plainText) {
        _logger.Log(new LoggerBinding<CryptoService> { Location = nameof(CreateHmacSha521Hash) });
        
        var plainTextStream = new MemoryStream();
        await plainTextStream.WriteAsync(plainText.EncodeDataUtf8());

        var sha512 = new HMACSHA512();
        var result = await sha512.ComputeHashAsync(plainTextStream);

        return result.DecodeUtf8<string>()!;
    }

    public async Task<string> CreateHmacMd5Hash(string plainText) {
        _logger.Log(new LoggerBinding<CryptoService> { Location = nameof(CreateHmacMd5Hash) });
        
        var plainTextStream = new MemoryStream();
        await plainTextStream.WriteAsync(plainText.EncodeDataUtf8());

        var md5 = new HMACMD5();
        var result = await md5.ComputeHashAsync(plainTextStream);
        
        return result.DecodeUtf8<string>()!;
    }
}