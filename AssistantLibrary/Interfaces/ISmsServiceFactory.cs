using AssistantLibrary.Interfaces.IServiceFactory;

namespace AssistantLibrary.Interfaces; 

public interface ISmsServiceFactory {

    ISmsService GetActiveSmsService();
}