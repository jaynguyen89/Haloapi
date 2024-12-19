namespace Halogen.Bindings.ViewModels;

public sealed class InterestVM {

    public string Id { get; set; } = null!;
    
    public string Name { get; set; } = null!;
    
    public string? Description { get; set; }
    
    public InterestVM? Parent { get; set; }
}