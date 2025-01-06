using HelperLibrary.Shared;

namespace Halogen.Bindings.ApiBindings;

public sealed class ProfilePhotoUpload {
    
    public bool IsAvatar { get; set; }

    public IFormFile Photo { get; set; } = null!;

    public List<string> VerifyData() {
        var errors = new List<string>();
        
        if (!Photo.ContentType.Contains("image")) errors.Add("The selected file is not an image.");
        else {
            var extension = Photo.ContentType.Split(Constants.FSlash).Last();
            if (!Constants.ImageTypes.Any(x => x.Contains(extension)))
                errors.Add($"Photo of type {extension} is not supported. Accepted types are {string.Join(Constants.Comma, Constants.ImageTypes.Select(x => x.Split(Constants.FSlash).Last()))}");
        }
        
        if (Photo.Length > Constants.ImageSize) errors.Add($"{nameof(Photo)} is too large. Max {Constants.ImageSize/1000000}MB.");
        return errors;
    }
}