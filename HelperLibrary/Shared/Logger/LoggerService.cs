using Microsoft.Extensions.Logging;

namespace HelperLibrary.Shared.Logger; 

public sealed class LoggerService: ILoggerService {

    private readonly ILogger<LoggerService> _logger;

    public LoggerService(ILogger<LoggerService> logger) {
        _logger = logger;
    }
    
    public void Log<T>(in LoggerBinding<T> binding) {
        var logString = binding.GetLogString();

        var loggingExpression = binding.Severity switch {
            Enums.LogSeverity.Information => (Func<object?>)(() => {
                _logger.LogInformation(logString);
                return default;
            }),
            Enums.LogSeverity.Debugging => () => {
                _logger.LogDebug(logString);
                return default;
            },
            Enums.LogSeverity.Error => () => {
                _logger.LogCritical(logString);
                return default;
            },
            _ => () => {
                _logger.LogWarning(logString);
                return default;
            }
        };

        _ = loggingExpression.Invoke();
    }
}