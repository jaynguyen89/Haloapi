namespace HelperLibrary.Shared.Logger; 

public interface ILoggerService {
    
    void Log<T>(in LoggerBinding<T> binding);
}