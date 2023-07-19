namespace AssistantLibrary;

public interface IAssistantServiceFactory {
    
    T? GetService<T>();
}