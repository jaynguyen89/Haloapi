namespace Halogen.Services.DbServices.Interfaces; 

public interface IContextService {
    
    /// <summary>
    /// To initiate a Transaction on the database.
    /// </summary>
    /// <returns>void</returns>
    Task StartTransaction();

    /// <summary>
    /// To commit changes in a Transaction on the database.
    /// </summary>
    /// <returns>void</returns>
    Task ConfirmTransaction();

    /// <summary>
    /// To discard changes in a Transaction on the database.
    /// </summary>
    /// <returns>void</returns>
    Task RevertTransaction();
}