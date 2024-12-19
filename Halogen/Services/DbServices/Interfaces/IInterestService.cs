using Halogen.Bindings.ViewModels;

namespace Halogen.Services.DbServices.Interfaces;

public interface IInterestService {
    
    Task<string[]?> GetAllIds();

    Task<InterestVM[]?> GetAllInterests();
}