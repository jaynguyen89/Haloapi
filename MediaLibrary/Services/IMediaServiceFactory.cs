namespace MediaLibrary.Services;

public interface IMediaServiceFactory {
    
    T? GetService<T>();
}