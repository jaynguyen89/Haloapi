using AssistantLibrary.Interfaces.IServiceFactory;

namespace AssistantLibrary.Interfaces; 

public interface ISmsServiceFactory {

    /// <summary>
    /// To get the active SMS service that is the concrete type of ISmsService.
    /// </summary>
    /// <returns>ISmsService</returns>
    ISmsService GetActiveSmsService();
}