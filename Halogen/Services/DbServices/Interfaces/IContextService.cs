namespace Halogen.Services.DbServices.Interfaces; 

internal interface IContextService {
    
    Task StartTransaction();

    Task ConfirmTransaction();

    Task RevertTransaction();
}