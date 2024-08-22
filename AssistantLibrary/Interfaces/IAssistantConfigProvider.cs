using AssistantLibrary.Bindings;

namespace AssistantLibrary.Interfaces;

public interface IAssistantConfigProvider {
    
    AssistantConfigs GetAssistantConfigs();
}