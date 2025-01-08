using MediaLibrary.Bindings;
using Microsoft.AspNetCore.Http;

namespace MediaLibrary.Services.Interfaces;

public interface IProfilePhotoService {
    
    /// <summary>
    /// To update a photo as profile avatar to HaloMedia.
    /// </summary>
    /// <param name="profileId">string</param>
    /// <param name="photo">IFormFile</param>
    /// <param name="currentAvatarName">string?</param>
    /// <returns>UploadResult?</returns>
    Task<UploadResult?> UploadAvatar(string profileId, IFormFile photo, string? currentAvatarName);
    
    /// <summary>
    /// To update a photo as profile cover to HaloMedia.
    /// </summary>
    /// <param name="profileId">string</param>
    /// <param name="photo">IFormFile</param>
    /// <param name="currentCoverName">string?</param>
    /// <returns>UploadResult?</returns>
    Task<UploadResult?> UploadCover(string profileId, IFormFile photo, string? currentCoverName);
    
    /// <summary>
    /// To delete a photo from HaloMedia.
    /// </summary>
    /// <param name="profileId">string</param>
    /// <param name="fileName">string</param>
    /// <param name="isAvatar">bool</param>
    /// <returns>bool?</returns>
    Task<bool?> DeletePhoto(string profileId, string fileName, bool isAvatar);
}