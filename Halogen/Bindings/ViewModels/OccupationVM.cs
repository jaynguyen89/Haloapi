using Halogen.DbModels;

namespace Halogen.Bindings.ViewModels;

public sealed class OccupationVM {
    
    public string Id { get; set; } = null!;
    
    public string Name { get; set; } = null!;
    
    public string? Description { get; set; }
    
    public OccupationVM? Parent { get; set; }
    
    public static implicit operator OccupationVM(Occupation occupation) => new() {
        Id = occupation.Id,
        Name = occupation.Name,
    };
}

public sealed class OccupationItemVM {
    
    public string Id { get; set; } = null!;
    
    public string? ParentId { get; set; }
    
    public string Name { get; set; } = null!;

    public static implicit operator OccupationItemVM(Occupation occupation) => new() {
        Id = occupation.Id,
        ParentId = occupation.ParentId,
        Name = occupation.Name,
    };
}