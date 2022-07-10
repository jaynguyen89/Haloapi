namespace HelperLibrary.Shared; 

public sealed class LoggerBinding<T> {

    public Enums.LogSeverity Severity { get; set; } = Enums.LogSeverity.INFORMATION;

    public string Location { get; set; } = null!;

    public string? Message { get; set; } = "Service starts.";
    
    public object? Data { get; set; }
}