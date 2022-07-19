using Newtonsoft.Json;

namespace HelperLibrary.Shared.Logger; 

public sealed class LoggerBinding<T> {

    public Enums.LogSeverity Severity { get; set; } = Enums.LogSeverity.INFORMATION;

    public string Location { get; set; } = null!;

    public string Message { get; set; } = "Service starts.";
    
    public object? Data { get; set; }

    public string GetLogString() =>
        $"{typeof(T).Name}.{Location}: {Message}" +
        $"{(Data is null ? string.Empty : $"\nData = {JsonConvert.SerializeObject(Data)}")}";
}