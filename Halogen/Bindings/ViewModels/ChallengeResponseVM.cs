using Halogen.DbModels;

namespace Halogen.Bindings.ViewModels;

public sealed class ChallengeResponseVM {

    public string Id { get; set; } = null!;
    
    public ChallengeVM Challenge { get; set; } = null!;
    
    public string Response { get; set; } = null!;
    
    public string UpdatedOn { get; set; } = null!;
}
