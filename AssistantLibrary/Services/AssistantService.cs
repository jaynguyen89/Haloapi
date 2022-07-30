using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AssistantLibrary.Bindings;
using AssistantLibrary.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Logger;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using QRCoder;

namespace AssistantLibrary.Services; 

public sealed class AssistantService: ServiceBase, IAssistantService {

    public AssistantService(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ): base(ecosystem, logger, configuration) { }
    
    public async Task<RecaptchaResponse?> IsHumanActivity(string clientToken) {
        _logger.Log(new LoggerBinding<AssistantService> { Location = nameof(IsHumanActivity) });
        var httpClient = new HttpClient();
        var (secretKey, endpoint, contentType) = (
            _configuration.AsEnumerable().Single(x => x.Key.Equals($"{_recaptchaBaseOptionKey}{nameof(AssistantLibraryOptions.Local.RecaptchaSettings.SecretKey)}")).Value,
            _configuration.AsEnumerable().Single(x => x.Key.Equals($"{_recaptchaBaseOptionKey}{nameof(AssistantLibraryOptions.Local.RecaptchaSettings.Endpoint)}")).Value,
            _configuration.AsEnumerable().Single(x => x.Key.Equals($"{_recaptchaBaseOptionKey}{nameof(AssistantLibraryOptions.Local.RecaptchaSettings.RequestContentType)}")).Value
        );

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
        var (size, darkColor, lightColor, eccLevel, withLogo, logoName) = (
            int.Parse(_configuration.AsEnumerable().Single(x => x.Key.Equals($"{_qrGeneratorBaseOptionKey}{nameof(AssistantLibraryOptions.Local.QrGeneratorSettings.ImageSize)}")).Value),
            _configuration.AsEnumerable().Single(x => x.Key.Equals($"{_qrGeneratorBaseOptionKey}{nameof(AssistantLibraryOptions.Local.QrGeneratorSettings.DarkColor)}")).Value,
            _configuration.AsEnumerable().Single(x => x.Key.Equals($"{_qrGeneratorBaseOptionKey}{nameof(AssistantLibraryOptions.Local.QrGeneratorSettings.LightColor)}")).Value,
            Enum.Parse<QRCodeGenerator.ECCLevel>(_configuration.AsEnumerable().Single(x => x.Key.Equals($"{_qrGeneratorBaseOptionKey}{nameof(AssistantLibraryOptions.Local.QrGeneratorSettings.EccLevel)}")).Value),
            bool.Parse(_configuration.AsEnumerable().Single(x => x.Key.Equals($"{_qrGeneratorBaseOptionKey}{nameof(AssistantLibraryOptions.Local.QrGeneratorSettings.WithLogo)}")).Value),
            _configuration.AsEnumerable().Single(x => x.Key.Equals($"{_qrGeneratorBaseOptionKey}{nameof(AssistantLibraryOptions.Local.QrGeneratorSettings.LogoName)}")).Value
        );

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