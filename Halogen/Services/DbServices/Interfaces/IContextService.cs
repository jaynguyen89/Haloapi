namespace Halogen.Services.DbServices.Interfaces; 

public interface IContextService {
    
    Task StartTransaction();

    Task ConfirmTransaction();

    Task RevertTransaction();
}