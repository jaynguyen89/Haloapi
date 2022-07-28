using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QRCoder;

namespace AssistantLibrary.Services; 

public sealed class AssistantService: ServiceBase, IAssistantService {

    public AssistantService(
        IEcosystem ecosystem,
        ILoggerService logger,
        IOptions<AssistantLibraryOptions> options
    ): base(ecosystem, logger, options) { }
    
    public async Task<RecaptchaResponse?> IsHumanActivity(string clientToken) {
        _logger.Log(new LoggerBinding<AssistantService> { Location = nameof(IsHumanActivity) });
        var httpClient = new HttpClient();

        var (secretKey, endpoint, contentType) = _environment switch {
            Constants.Development => (_options.Dev.RecaptchaSettings.SecretKey, _options.Dev.RecaptchaSettings.Endpoint, _options.Dev.RecaptchaSettings.RequestContentType),
            Constants.Staging => (_options.Stg.RecaptchaSettings.SecretKey, _options.Stg.RecaptchaSettings.Endpoint, _options.Stg.RecaptchaSettings.RequestContentType),
            Constants.Production => (_options.Prod.RecaptchaSettings.SecretKey, _options.Prod.RecaptchaSettings.Endpoint, _options.Prod.RecaptchaSettings.RequestContentType),
            _ => (_options.Loc.RecaptchaSettings.SecretKey, _options.Loc.RecaptchaSettings.Endpoint, _options.Loc.RecaptchaSettings.RequestContentType)
        };

        httpClient.BaseAddress = new Uri(endpoint);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypes[contentType]));

        var httpResponse = await httpClient.PostAsJsonAsync($"?secret={secretKey}&response={clientToken}", HttpCompletionOption.ResponseContentRead);
        return !httpResponse.IsSuccessStatusCode
            ? new RecaptchaResponse()
            : JsonConvert.DeserializeObject<RecaptchaResponse>(await httpResponse.Content.ReadAsStringAsync());
    }

    public byte[] GenerateQrImage(string information, FileStream? image) {
        _logger.Log(new LoggerBinding<AssistantService> { Location = nameof(GenerateQrImage) });
        var (size, darkColor, lightColor, eccLevel, withLogo, logoName) = _environment switch {
            Constants.Development => (
                int.Parse(_options.Dev.QrGeneratorSettings.ImageSize),
                _options.Dev.QrGeneratorSettings.DarkColor,
                _options.Dev.QrGeneratorSettings.LightColor,
                Enum.Parse<QRCodeGenerator.ECCLevel>(_options.Dev.QrGeneratorSettings.EccLevel),
                bool.Parse(_options.Dev.QrGeneratorSettings.WithLogo),
                _options.Dev.QrGeneratorSettings.LogoName
            ),
            Constants.Staging => (
                int.Parse(_options.Stg.QrGeneratorSettings.ImageSize),
                _options.Stg.QrGeneratorSettings.DarkColor,
                _options.Stg.QrGeneratorSettings.LightColor,
                Enum.Parse<QRCodeGenerator.ECCLevel>(_options.Stg.QrGeneratorSettings.EccLevel),
                bool.Parse(_options.Stg.QrGeneratorSettings.WithLogo),
                _options.Stg.QrGeneratorSettings.LogoName
            ),
            Constants.Production => (
                int.Parse(_options.Prod.QrGeneratorSettings.ImageSize),
                _options.Prod.QrGeneratorSettings.DarkColor,
                _options.Prod.QrGeneratorSettings.LightColor,
                Enum.Parse<QRCodeGenerator.ECCLevel>(_options.Prod.QrGeneratorSettings.EccLevel),
                bool.Parse(_options.Prod.QrGeneratorSettings.WithLogo),
                _options.Prod.QrGeneratorSettings.LogoName
            ),
            _ => (
                int.Parse(_options.Loc.QrGeneratorSettings.ImageSize),
                _options.Loc.QrGeneratorSettings.DarkColor,
                _options.Loc.QrGeneratorSettings.LightColor,
                Enum.Parse<QRCodeGenerator.ECCLevel>(_options.Loc.QrGeneratorSettings.EccLevel),
                bool.Parse(_options.Loc.QrGeneratorSettings.WithLogo),
                _options.Loc.QrGeneratorSettings.LogoName
            )
        };

        var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(information, eccLevel);

        var qrCode = new QRCode(qrData);
        Bitmap qrImage;
        if (image is not null || withLogo) {
            var htmlDarkColor = ColorTranslator.FromHtml(darkColor);
            var htmlLightColor = ColorTranslator.FromHtml(lightColor);
            var logoFile = image ?? new FileStream($"{Constants.AssetsDirectoryPath}{logoName}", FileMode.Open, FileAccess.Read);

            qrImage = qrCode.GetGraphic(size, htmlDarkColor, htmlLightColor, new Bitmap(logoFile));
        }
        else qrImage = qrCode.GetGraphic(size, darkColor, lightColor);

        using var memoryStream = new MemoryStream();
        qrImage.Save(memoryStream, ImageFormat.Png);

        return memoryStream.ToArray();
    }
}