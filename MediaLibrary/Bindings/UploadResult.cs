namespace MediaLibrary.Bindings;

public sealed class UploadResult {
    
    public bool IsSuccess { get; set; }
    
    public string? Message { get; set; }
    
    public string? FileName { get; set; }
}