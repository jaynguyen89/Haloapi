using MediaLibrary.Bindings;
using Microsoft.AspNetCore.Http;

namespace MediaLibrary.Services.Interfaces;

public interface IProfilePhotoService {
    
    Task<UploadResult?> UploadAvatar(string profileId, IFormFile photo, string? currentAvatarName);
    
    Task<UploadResult?> UploadCover(string profileId, IFormFile photo, string? currentCoverName);
    
    Task<bool?> DeletePhoto(string profileId, string fileName, bool isAvatar);
}