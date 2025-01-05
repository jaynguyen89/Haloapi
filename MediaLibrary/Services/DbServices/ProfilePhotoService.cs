using System.Net.Http.Headers;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using MediaLibrary.Bindings;
using MediaLibrary.DbContexts;
using MediaLibrary.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Constants = HelperLibrary.Shared.Constants;

namespace MediaLibrary.Services.DbServices;

public sealed class ProfilePhotoService: ServiceBase, IProfilePhotoService {

    private const string AvatarController = "AvatarPhoto";
    private const string CoverController = "CoverPhoto";
    
    public ProfilePhotoService(
        ILoggerService logger,
        IConfiguration configuration,
        MediaLibraryDbContext dbContext,
        string environment
    ): base(logger, configuration, dbContext, environment) { }

    public async Task<UploadResult?> UploadAvatar(string profileId, IFormFile photo, string? currentAvatarName) {
        _logger.Log(new LoggerBinding<ProfilePhotoService> { Location = nameof(UploadAvatar) });

        var targetApi = !currentAvatarName.IsString()
            ? $"{AvatarController}{Constants.FSlash}saveAvatarPhoto"
            : $"{AvatarController}{Constants.FSlash}replaceAvatarPhoto";
        
        var apiToken = await SetApiToken(profileId, targetApi);
        if (!apiToken.IsString()) return default;

        var photoStream = photo.OpenReadStream();
        var formFile = new StreamContent(photoStream);
        formFile.Headers.ContentType = MediaTypeHeaderValue.Parse(photo.ContentType);

        var formData = new MultipartFormDataContent {
            { formFile, "avatar", photo.FileName }
        };
        
        formData.Headers.ContentType = MediaTypeHeaderValue.Parse(photo.ContentType);
        formData.Headers.Add("AccountId", profileId);
        formData.Headers.Add("ApiToken", apiToken);
        formData.Headers.Add("Target", targetApi);

        var endpoint = !currentAvatarName.IsString()
            ? "avatar/save"
            : $"avatar/replace/{currentAvatarName}";
        
        var result = await _httpClient.PostAsync(endpoint, formData);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(await result.Content.ReadAsStringAsync());
        
        photoStream.Close();
        return new UploadResult {
            IsSuccess = result.IsSuccessStatusCode,
            Message = apiResponse?.Message ?? string.Empty,
            FileName = apiResponse?.Message ?? string.Empty,
        };
    }
    
    public async Task<UploadResult?> UploadCover(string profileId, IFormFile photo, string? currentCoverName) {
        _logger.Log(new LoggerBinding<ProfilePhotoService> { Location = nameof(UploadCover) });

        var targetApi = !currentCoverName.IsString()
            ? $"{CoverController}{Constants.FSlash}saveCoverPhoto"
            : $"{CoverController}{Constants.FSlash}replaceCoverPhoto";
        
        var apiToken = await SetApiToken(profileId, targetApi);
        if (!apiToken.IsString()) return default;
        
        var photoStream = photo.OpenReadStream();
        var formFile = new StreamContent(photoStream);
        formFile.Headers.ContentType = MediaTypeHeaderValue.Parse(photo.ContentType);

        var formData = new MultipartFormDataContent {
            { formFile, "cover", photo.FileName }
        };
        
        formData.Headers.ContentType = MediaTypeHeaderValue.Parse(photo.ContentType);
        formData.Headers.Add("AccountId", profileId);
        formData.Headers.Add("ApiToken", apiToken);
        formData.Headers.Add("Target", targetApi);
        
        var endpoint = !currentCoverName.IsString()
            ? "cover/save"
            : $"cover/replace/{currentCoverName}";
        
        var result = await _httpClient.PostAsync(endpoint, formData);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(await result.Content.ReadAsStringAsync());
        
        photoStream.Close();
        return new UploadResult {
            IsSuccess = result.IsSuccessStatusCode,
            Message = apiResponse?.Message ?? string.Empty,
            FileName = apiResponse?.Message ?? string.Empty,
        };
    }
    
    public async Task<bool?> DeletePhoto(string profileId, string fileName, bool isAvatar) {
        _logger.Log(new LoggerBinding<ProfilePhotoService> { Location = nameof(DeletePhoto) });

        var targetApi = !isAvatar
            ? $"{CoverController}{Constants.FSlash}deleteCoverPhoto"
            : $"{AvatarController}{Constants.FSlash}deleteAvatarPhoto";
        
        var apiToken = await SetApiToken(profileId, targetApi);
        if (!apiToken.IsString()) return default;

        var endpoint = !isAvatar ? $"cover/delete/{fileName}" : $"avatar/delete/{fileName}";
        
        var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        request.Headers.Add("AccountId", profileId);
        request.Headers.Add("ApiToken", apiToken);
        request.Headers.Add("Target", targetApi);
        
        var result = await _httpClient.SendAsync(request);
        return result.IsSuccessStatusCode;
    }
}