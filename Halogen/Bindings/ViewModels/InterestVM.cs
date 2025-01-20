using Halogen.DbModels;

namespace Halogen.Bindings.ViewModels;

public sealed class InterestVM {

    public string Id { get; set; } = null!;
    
    public string Name { get; set; } = null!;
    
    public string? Description { get; set; }
    
    public bool IsHobby { get; set; }
    
    public InterestVM? Parent { get; set; }

    public static implicit operator InterestVM(Interest interest) => new() {
        Id = interest.Id,
        Name = interest.Name,
        IsHobby = interest.IsHobby,
    };
}

public sealed class InterestItemVM {

    public string Id { get; set; } = null!;
    
    public string? ParentId { get; set; }
    
    public string Name { get; set; } = null!;

    public static implicit operator InterestItemVM(Interest interest) => new() {
        Id = interest.Id,
        ParentId = interest.ParentId,
        Name = interest.Name,
    };
}