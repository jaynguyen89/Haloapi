namespace Halogen.Bindings.ViewModels;

public sealed class ChallengeVM {

    public string Id { get; set; } = null!;
    
    public string Question { get; set; } = null!;
    
    public string? CreatedOn { get; set; }
    
    public CreatedByVM? CreatedBy { get; set; }
}
