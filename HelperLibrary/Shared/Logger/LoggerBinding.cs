﻿using HelperLibrary.Shared.Helpers;
using Newtonsoft.Json;

namespace HelperLibrary.Shared.Logger; 

public sealed class LoggerBinding<T> {

    public Enums.LogSeverity Severity { get; set; } = Enums.LogSeverity.Information;

    public string Location { get; set; } = null!;

    public bool IsPrivate { get; set; } = false;

    public string Message { get; set; } = "Service starts.";
    
    public object? Data { get; set; }
    
    public Exception? E { get; set; }

    public string GetLogString() =>
        $"<<{Severity.GetValue()!.ToUpper()}>> {(IsPrivate ? "private " : "")}" +
        $"{typeof(T).Name}.{Location}: {Message}" +
        $"{(Data is null ? string.Empty : $"\nData = {JsonConvert.SerializeObject(Data)}")}" +
        $"{(E is null ? string.Empty : $"\nException = {JsonConvert.SerializeObject(E)}")}";
}